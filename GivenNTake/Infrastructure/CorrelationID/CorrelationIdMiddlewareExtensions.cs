using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiveNTake.Infrastructure.CorrelationID
{
    public static class CorrelationIdMiddlewareExtensions
    {
        public static void UseCorrelationIdHeader(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdHeaderMiddleware>();
        }
    }
}
