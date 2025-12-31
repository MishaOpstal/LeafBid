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

using LeafBidAPI.Data.seeders;

namespace LeafBidAPI.Data;

public class SeederEngine(ILogger<SeederEngine> logger)
{
    public async Task<bool> RunAsync(string[] seederNames, ISeeder[] allSeeders)
    {
        ISeeder[] seeders = allSeeders.Where(s => seederNames.Contains(s.GetType().Name)).ToArray();
        return await RunAsync(seeders);
    }

    public async Task<bool> RunAsync(ISeeder[] seeders)
    {
        if (seeders.Length == 0)
        {
            logger.LogInformation("No Seeders to apply.");
            return false;
        }

        logger.LogInformation("Start seeding the database...");
        string seederName = string.Empty;
        bool appliedAny = false;
        try
        {
            foreach (ISeeder seeder in seeders)
            {
                seederName = seeder.GetType().Name;
                logger.LogInformation("--> Applying Seeder: {SeederName}", seederName);
                await seeder.SeedAsync();
                logger.LogInformation("--> Applied Seeder: {SeederName}", seederName);
                appliedAny = true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying the Seeder: {SeederName}", seederName);
        }
        return appliedAny;
    }
}