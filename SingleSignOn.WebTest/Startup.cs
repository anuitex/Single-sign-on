using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Security.Google;
using SingleSignOn.BusinessLogic.Interfaces;
using SingleSignOn.BusinessLogic.Services;
using SingleSignOn.DataAccess;
using SingleSignOn.DataAccess.Entities;
using System;

namespace SingleSignOn.WebTest
{
    public class Startup
    {
        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SingleSignOn")));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = "298569871907-einlhmd6j2uvp4reh8k0o1b0e2evitso.apps.googleusercontent.com";
                googleOptions.ClientSecret = "YPJHD6kv7RXSco9ga48nu-JK";
            });
            //services.AddAuthentication().AddGoogle(googleOptions =>
            //{
            //    googleOptions.ClientId = "298569871907-einlhmd6j2uvp4reh8k0o1b0e2evitso.apps.googleusercontent.com";
            //    googleOptions.ClientSecret = "YPJHD6kv7RXSco9ga48nu-JK";
            //});
            //services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();
            
            var autoFacBuilder = new ContainerBuilder();
            autoFacBuilder.RegisterType<AccountService>().As<IAccountService>();
            autoFacBuilder.Populate(services);
            var container = autoFacBuilder.Build();

            return new AutofacServiceProvider(container);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
           
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
