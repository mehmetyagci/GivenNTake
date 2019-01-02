using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GivenNTake.Data;
using GivenNTake.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GivenNTake
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration;


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem

            services.AddCors(); // add the CORS middleware
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });           

            services.AddDbContext<GiveNTakeContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("GiveNTakeDB")));

            services.AddIdentity<User, IdentityRole>()
               .AddEntityFrameworkStores<GiveNTakeContext>()
               .AddDefaultTokenProviders();

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateActor = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = Configuration["JWTConfiguration:Issuer"],
                    ValidAudience = Configuration["JWTConfiguration:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWTConfiguration:SigningKey"]))
                };
            });

            services.AddAuthorization(options => options.AddPolicy("ExperiencedUser", (AuthorizationPolicyBuilder policy) =>
               policy.RequireAssertion(context =>
               {
                   var registrationClaimValue = context.User.Claims.SingleOrDefault(c => c.Type == "registration-date")?.Value;
                   if (DateTime.TryParseExact(registrationClaimValue, "yy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var registrationTime))
                   {
                       return registrationTime.AddYears(1) < DateTime.UtcNow;
                   }
                   return false;
               })));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            UpdateDatabase(app);
            app.UseCors(b =>
            {
                b.AllowAnyHeader();
                b.AllowAnyOrigin();
                b.AllowAnyMethod();
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();                       
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var roleManager = services.GetService<RoleManager<IdentityRole>>();
                using (var context = serviceScope.ServiceProvider.GetService<GiveNTakeContext>())
                {
                    context.Database.Migrate();
                    context.SeedData();
                    context.SeedRoleAsync(roleManager).Wait();
                }
            }
        }
    }
}
