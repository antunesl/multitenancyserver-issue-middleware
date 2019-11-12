using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenancyServer;
using System;
using System.Linq;

namespace ModularApp
{
    public class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Seeding database...");

            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();

                var tenantMgr = scope.ServiceProvider.GetRequiredService<TenantManager<ApplicationTenant>>();
                var tenant1 = tenantMgr.FindByCanonicalNameAsync("tenant1").Result;
                if (tenant1 == null)
                {
                    tenant1 = new ApplicationTenant
                    {
                        CanonicalName = "tenant1",
                        DisplayName = "Tenant One",
                    };
                    var result = tenantMgr.CreateAsync(tenant1).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("tenant1 created");
                }
                else
                {
                    Console.WriteLine("tenant1 already exists");
                }

                var tenant2 = tenantMgr.FindByCanonicalNameAsync("tenant2").Result;
                if (tenant2 == null)
                {
                    tenant2 = new ApplicationTenant
                    {
                        CanonicalName = "tenant2",
                        DisplayName = "Tenant Two",
                    };
                    var result = tenantMgr.CreateAsync(tenant2).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("tenant2 created");
                }
                else
                {
                    Console.WriteLine("tenant2 already exists");
                }
            }



            Console.WriteLine("Done seeding database.");
            Console.WriteLine();
        }
    }
}
