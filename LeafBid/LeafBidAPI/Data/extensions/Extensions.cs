/*
    MIT License

    Copyright (c) 2024 Djoufson CHE BENE

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using System.Reflection;
using LeafBidAPI.Data.seeders;

namespace LeafBidAPI.Data.extensions;

public static class Extensions
{
    #region RegisterSeeders
    public static IServiceCollection ConfigureSeedersEngine(this IServiceCollection services)
    {
        services.AddScoped<SeederEngine>();
        return services;
    }

    public static IServiceCollection AddSeeder<TSeeder>(this IServiceCollection services)
        where TSeeder : class, ISeeder
    {
        services.AddScoped<ISeeder, TSeeder>();
        return services;
    }

    public static IServiceCollection AddSeedersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        IEnumerable<Type> seederTypes = assembly.GetTypes()
            .Where(t => typeof(ISeeder).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false });

        foreach (Type seederType in seederTypes)
        {
            services.AddScoped(typeof(ISeeder), seederType);
        }

        return services;
    }

    public static IServiceCollection AddSeedersFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            services.AddSeedersFromAssembly(assembly);
        }

        return services;
    }
    #endregion

    #region Seed Commands
    public static async Task<bool> MapSeedCommandsAsync(this IApplicationBuilder app, string[] args)
    {
        if (args.Length == 0)
        {
            return false;
        }

        if (!args[0].Equals("--seed", StringComparison.CurrentCultureIgnoreCase)
            && !args[0].Equals("-s", StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        using IServiceScope scope = app.ApplicationServices.CreateScope();
        SeederEngine seederEngine = scope.ServiceProvider.GetRequiredService<SeederEngine>();
        IEnumerable<ISeeder> seeders = scope.ServiceProvider.GetServices<ISeeder>();
        if (args.Length == 1)
        {
            return await seederEngine.RunAsync(seeders.ToArray());
        }

        string[] seederNames = args.Skip(1).ToArray();
        return await seederEngine.RunAsync(seederNames, seeders.ToArray());
    }
    #endregion
}