using GiveNTake.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiveNTake.Extensions
{
    public static class WebHostExtensions
    {
        public static IWebHost MigrateCatalog(this IWebHost host, bool ensureDeleted = false)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetService<GiveNTakeContext>();
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    // Error handling
                    throw;
                }
            }
            return host;
        }

    }
}
