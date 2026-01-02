using System.Reflection;
using LeafBidAPI.Data;
using LeafBidAPI.Data.extensions;
using LeafBidAPI.Data.seeders;
using LeafBidAPI.Filters;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using LeafBidAPI.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace LeafBidAPI;

public class Program
{
    public static void Main(string[] args)
    {
        string allowedOrigins = "_allowedOrigins";
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
                        // Needed to allow the browser to send/receive the refresh-token cookie
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

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddRouting();
        builder.Services.AddHttpClient();

        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

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

        // Add Authentication (Bearer OR Identity.Application cookie)
        builder.Services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies(identityCookies =>
            {
                identityCookies.ApplicationCookie?.Configure(options =>
                {
                    options.Cookie.Name = ".AspNetCore.Identity.Application";

                    // For same-origin SPA calls (recommended) this is perfect.
                    options.Cookie.SameSite = SameSiteMode.Lax;

                    // If you run ONLY http://localhost, keep SameAsRequest.
                    // If you run HTTPS, this will become Secure automatically.
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

        // Set-up versioning for Swagger
        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; // format: 'v'major[.minor][.patch]
            options.SubstituteApiVersionInUrl = true;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

        //Role seeding
        using (IServiceScope scope = app.Services.CreateScope())
        {
            RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = ["Admin", "Buyer", "Provider", "Auctioneer"];
            foreach (string role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    roleManager.CreateAsync(new IdentityRole(role)).Wait();
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

        // app.MapIdentityApi<User>();
        app.MapControllers();
        app.UseStaticFiles();

        // Check for seed commands
        bool appliedAny = app.MapSeedCommandsAsync(args).Result;
        if (appliedAny)
        {
            // Prevent the app from continuing further.
            return;
        }
        
        app.Run();
    }
}