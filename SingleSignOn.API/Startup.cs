using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SingleSignOn.BusinessLogic.Interfaces;
using SingleSignOn.BusinessLogic.Services;
using SingleSignOn.DataAccess;
using SingleSignOn.DataAccess.Entities;
using System;
using System.Text;

namespace SingleSignOn.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Configuration = configuration;

            //    var builder = new ConfigurationBuilder()
            //.SetBasePath(environment.ContentRootPath)
            //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
            //.AddEnvironmentVariables();
            //    this.Configuration = builder.Build();
        }


        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SingleSignOn")));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/");

            services.AddMvc();

            var autoFacBuilder = new ContainerBuilder();
            autoFacBuilder.RegisterType<AccountService>().As<IAccountService>();
            autoFacBuilder.Populate(services);
            var container = autoFacBuilder.Build();

            var appSettings = Configuration.GetSection("AppSettings");
            var authTokenProviderOptions = Configuration.GetSection("AuthTokenProviderOptions");
            var secretKey = appSettings?["JwtKey"] ?? "default_secret_key";
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var validIssuer = authTokenProviderOptions?["Issuer"];

            services.AddAuthentication(o =>
            {
                o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Account/Login/");
                options.LoginPath = new PathString("/Account/Login/");
                //options.LoginPath = new PathString("/Areas/Account/Controllers/Account/Login/");
            });

            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = "603900993327-fc5df5mb9vt2vka5i3e95f4gir2qc87g.apps.googleusercontent.com";
                googleOptions.ClientSecret = "j_qZsLntmtUL1R7q2ch3SLZl";
            });
            

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,

                    ValidateIssuer = true,
                    ValidIssuer = validIssuer,

                    ValidateAudience = true,
                    ValidAudience = "",

                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.Zero
                };
                o.RequireHttpsMetadata = false;
            });

            return new AutofacServiceProvider(container);

            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("SingleSignOn")));

            //services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            //{
            //    options.Password.RequireDigit = false;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireLowercase = false;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = false;
            //}).AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

            //services.AddLogging();
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //        builder => builder.AllowAnyOrigin()
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowCredentials());
            //});
            //services.Configure<MvcOptions>(options =>
            //{
            //    options.Filters.Add(new CorsAuthorizationFilterFactory("CorsPolicy"));
            //});
            ////services.AddTransient<IEmailSender, EmailSender>();

            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //});
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new Info { Title = "Contacts API", Version = "v1" });
            //    c.AddSecurityDefinition("Bearer", new ApiKeyScheme
            //    {
            //        In = "header",
            //        Description = "Please insert JWT with Bearer into field",
            //        Name = "Authorization",
            //        Type = "apiKey"
            //    });
            //});
            //services.AddMvc();

            //var autoFacBuilder = new ContainerBuilder();
            //autoFacBuilder.RegisterType<AccountService>().As<IAccountService>();
            //autoFacBuilder.Populate(services);
            //var container = autoFacBuilder.Build();

            //return new AutofacServiceProvider(container);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //context.Database.Migrate();
                context.Database.EnsureCreated();
            }

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseBrowserLink();
            //    app.UseDatabaseErrorPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //}

            //app.UseStaticFiles();
            //app.UseCors("CorsPolicy");
            //app.UseAuthentication();
            //app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contacts API V1");
            //});
            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //         name: "api",
            //         template: "{area=Account}/{controller=Account}/{action=Login}/{id?}");

            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Account}/{action=Login}/{id?}");
            //});
        }
    }
}
