using System.Reflection;
using System.Security.Claims;
using LeafBidAPI.Configuration;
using LeafBidAPI.Data;
using LeafBidAPI.Data.extensions;
using LeafBidAPI.Data.seeders;
using LeafBidAPI.Filters;
using LeafBidAPI.Helpers;
using LeafBidAPI.Hubs;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using LeafBidAPI.Permissions;
using LeafBidAPI.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace LeafBidAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        const string allowedOrigins = "_allowedOrigins";

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(8080); // match Docker container port
        });

        // Add services to the container.
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/app/dpkeys"))
            .SetApplicationName("LeafBidAPI");

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: allowedOrigins,
                policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        // Dependency Injection for Services
        builder.Services.AddScoped<IAuctionService, AuctionService>();
        builder.Services.AddScoped<IAuctionSaleService, AuctionSaleService>();
        builder.Services.AddScoped<IAuctionSaleProductService, AuctionSaleProductService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IPagesServices, PagesServices>();
        builder.Services.AddScoped<AuctionHelper>();
        builder.Services.AddHostedService<AuctionStatusWorker>();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(PolicyTypes.Auctions.View, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, AuctionPermissions.View);
            })
            .AddPolicy(PolicyTypes.Auctions.Manage, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, AuctionPermissions.View);
                policy.RequireClaim(ApplicationClaimType.Permission, AuctionPermissions.Create);
                policy.RequireClaim(ApplicationClaimType.Permission, AuctionPermissions.Update);
                policy.RequireClaim(ApplicationClaimType.Permission, AuctionPermissions.Start);
                policy.RequireClaim(ApplicationClaimType.Permission, AuctionPermissions.Stop);
            })
            .AddPolicy(PolicyTypes.AuctionSales.View, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, AuctionSalePermissions.View);
            })
            .AddPolicy(PolicyTypes.Products.View, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, ProductPermissions.View);
            })
            .AddPolicy(PolicyTypes.Products.Buy, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, ProductPermissions.View);
                policy.RequireClaim(ApplicationClaimType.Permission, ProductPermissions.Buy);
            })
            .AddPolicy(PolicyTypes.Products.Manage, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, ProductPermissions.View);
                policy.RequireClaim(ApplicationClaimType.Permission, ProductPermissions.Create);
                policy.RequireClaim(ApplicationClaimType.Permission, ProductPermissions.Register);
                policy.RequireClaim(ApplicationClaimType.Permission, ProductPermissions.Update);
                policy.RequireClaim(ApplicationClaimType.Permission, ProductPermissions.Delete);
            })
            .AddPolicy(PolicyTypes.Companies.View, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, CompanyPermissions.View);
            })
            .AddPolicy(PolicyTypes.Companies.Manage, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, CompanyPermissions.View);
                policy.RequireClaim(ApplicationClaimType.Permission, CompanyPermissions.Create);
                policy.RequireClaim(ApplicationClaimType.Permission, CompanyPermissions.Update);
            })
            .AddPolicy(PolicyTypes.Roles.ViewOthers, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, RolePermissions.ViewOthers);
            })
            .AddPolicy(PolicyTypes.Roles.Manage, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, RolePermissions.ViewOthers);
                policy.RequireClaim(ApplicationClaimType.Permission, RolePermissions.Manage);
            })
            .AddPolicy(PolicyTypes.Users.ViewOthers, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, UserPermissions.ViewOthers);
            })
            .AddPolicy(PolicyTypes.Users.Manage, policy =>
            {
                policy.RequireClaim(ApplicationClaimType.Permission, UserPermissions.ViewOthers);
                policy.RequireClaim(ApplicationClaimType.Permission, UserPermissions.Manage);
            });

        builder.Services.AddControllers();
        builder.Services.AddRouting();
        builder.Services.AddSignalR();
        builder.Services.AddHttpClient();

        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services.Configure<AuctionTimerSettings>(builder.Configuration.GetSection("AuctionTimer"));

        builder.Services
            .AddOptions<AuctionTimerSettings>()
            .Bind(builder.Configuration)
            .ValidateDataAnnotations()
            .Validate(
                s => s.MinDurationForAuctionTimer > 0,
                "MinDurationForAuctionTimer must be positive"
            )
            .ValidateOnStart();

        builder.Services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Add Authentication (Identity.Application cookie)
        builder.Services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies(identityCookies =>
            {
                identityCookies.ApplicationCookie?.Configure(options =>
                {
                    options.Cookie.Name = ".AspNetCore.Identity.Application";
                    options.Cookie.SameSite = SameSiteMode.Lax;

                    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                        ? CookieSecurePolicy.SameAsRequest
                        : CookieSecurePolicy.Always;

                    // Prevent redirects in APIs
                    options.Events.OnRedirectToLogin = ctx =>
                        Task.FromResult(ctx.Response.StatusCode = StatusCodes.Status401Unauthorized);
                    options.Events.OnRedirectToAccessDenied = ctx =>
                        Task.FromResult(ctx.Response.StatusCode = StatusCodes.Status403Forbidden);
                });
            });

        builder.Services.AddScoped<RoleManager<IdentityRole>>();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddTransient<IEmailSender<User>, DummyEmailSender>();
        }
        else
        {
            builder.Services.AddTransient<IEmailSender<User>, EmailSenderService>();
        }

        builder.Services.ConfigureSeedersEngine();
        builder.Services.AddSeeder<CompanySeeder>();
        builder.Services.AddSeeder<UserSeeder>();
        builder.Services.AddSeeder<ProductSeeder>();
        builder.Services.AddSeeder<RegisteredProductSeeder>();
        builder.Services.AddSeeder<AuctionSeeder>();

        // Set-up versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        // Set up versioning for Swagger
        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; // format: 'v'major[.minor][.patch]
            options.SubstituteApiVersionInUrl = true;
        });

        // Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v2", new OpenApiInfo { Title = "LeafBidAPI", Version = "v2" });

            string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            c.SchemaFilter<EnumSchemaFilter>();
        });

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "LeafBidAPI V2");
            });
        }

        // Role + role-claim seeding (FIXED)
        using (IServiceScope scope = app.Services.CreateScope())
        {
            RoleManager<IdentityRole> roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = ["Admin", "Buyer", "Provider", "Auctioneer"];

            foreach (string roleName in roles)
            {
                IdentityRole? roleEntity = await roleManager.FindByNameAsync(roleName);

                if (roleEntity == null)
                {
                    roleEntity = new IdentityRole(roleName);
                    IdentityResult createResult = await roleManager.CreateAsync(roleEntity);

                    if (!createResult.Succeeded)
                    {
                        continue;
                    }
                }

                async Task AddPermissionAsync(string permission)
                {
                    IList<Claim> existingClaims = await roleManager.GetClaimsAsync(roleEntity);

                    if (existingClaims.Any(c =>
                            c.Type == ApplicationClaimType.Permission &&
                            c.Value == permission))
                    {
                        return;
                    }

                    await roleManager.AddClaimAsync(
                        roleEntity,
                        new Claim(ApplicationClaimType.Permission, permission)
                    );
                }

                switch (roleName)
                {
                    case "Admin":
                        await AddPermissionAsync(AuctionPermissions.View);
                        await AddPermissionAsync(AuctionPermissions.Create);
                        await AddPermissionAsync(AuctionPermissions.Update);
                        await AddPermissionAsync(AuctionPermissions.Start);
                        await AddPermissionAsync(AuctionPermissions.Stop);

                        await AddPermissionAsync(AuctionSalePermissions.View);

                        await AddPermissionAsync(ProductPermissions.Buy);
                        await AddPermissionAsync(ProductPermissions.View);
                        await AddPermissionAsync(ProductPermissions.Create);
                        await AddPermissionAsync(ProductPermissions.Register);
                        await AddPermissionAsync(ProductPermissions.Update);
                        await AddPermissionAsync(ProductPermissions.Delete);

                        await AddPermissionAsync(CompanyPermissions.View);
                        await AddPermissionAsync(CompanyPermissions.Create);
                        await AddPermissionAsync(CompanyPermissions.Update);

                        await AddPermissionAsync(RolePermissions.ViewOthers);
                        await AddPermissionAsync(RolePermissions.Manage);
                        break;

                    case "Buyer":
                        await AddPermissionAsync(AuctionPermissions.View);
                        await AddPermissionAsync(ProductPermissions.Buy);
                        await AddPermissionAsync(ProductPermissions.View);
                        await AddPermissionAsync(RolePermissions.ViewOthers);
                        break;

                    case "Provider":
                        await AddPermissionAsync(AuctionPermissions.View);
                        await AddPermissionAsync(ProductPermissions.View);
                        await AddPermissionAsync(ProductPermissions.Create);
                        await AddPermissionAsync(ProductPermissions.Register);
                        await AddPermissionAsync(ProductPermissions.Update);
                        await AddPermissionAsync(ProductPermissions.Delete);

                        await AddPermissionAsync(RolePermissions.ViewOthers);
                        break;

                    case "Auctioneer":
                        await AddPermissionAsync(AuctionPermissions.View);
                        await AddPermissionAsync(AuctionPermissions.Create);
                        await AddPermissionAsync(AuctionPermissions.Update);
                        await AddPermissionAsync(AuctionPermissions.Start);
                        await AddPermissionAsync(AuctionPermissions.Stop);

                        await AddPermissionAsync(AuctionSalePermissions.View);
                        await AddPermissionAsync(ProductPermissions.View);

                        await AddPermissionAsync(RolePermissions.ViewOthers);
                        break;
                }
            }
        }

        // Configure HTTPS if not in development
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseCors(allowedOrigins);

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<AuctionHub>("/auctionHub");
        app.UseStaticFiles();

        // Check for seed commands
        bool appliedAny = await app.MapSeedCommandsAsync(args);
        if (appliedAny)
        {
            return;
        }

        app.Run();
    }
}
