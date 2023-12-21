// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Seer.Hubs;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.Services;

namespace Seer
{
    public class Startup
    {
        private Configuration _conf { get; }
        private bool _isDevelopment { get; set; }

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            _conf = ConfigurationService.Load();
            _isDevelopment = env.IsDevelopment();
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            services
                .AddApplicationData(_conf.Database.ConnectionString)
                .AddApplicationServices();

            services.AddIdentityCore<User>(options =>
                {
                    // very lax for training and exercise purposes only
                    options.Password.RequiredLength = 5;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager<SignInManager<User>>()
                .AddDefaultTokenProviders();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "SEER API",
                    Description = "SEER API",
                    Contact = new OpenApiContact
                    {
                        Name = "Dustin Updyke",
                        Email = "ddupdyke@sei.cmu.edu"
                    },
                    License = new OpenApiLicense
                    {
                        Name =
                            $"Copyright 2021 Carnegie Mellon University. All Rights Reserved. See license.md file for terms"
                    }
                });
            });

            services.AddSingleton<IFileProvider>(
                new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "App_Data")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddAuthentication(delegate (AuthenticationOptions options) {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddCookie(IdentityConstants.ApplicationScheme, delegate (CookieAuthenticationOptions o) {
                o.LoginPath = new PathString("/account/login");
                CookieAuthenticationEvents events1 = new CookieAuthenticationEvents();
                events1.OnValidatePrincipal = new Func<CookieValidatePrincipalContext, Task>(SecurityStampValidator.ValidatePrincipalAsync);
                o.Events = events1;
            }).AddCookie(IdentityConstants.ExternalScheme, delegate (CookieAuthenticationOptions o) {
                o.Cookie.Name = IdentityConstants.ExternalScheme;
            }).AddCookie(IdentityConstants.TwoFactorRememberMeScheme, delegate (CookieAuthenticationOptions o) {
                o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
                CookieAuthenticationEvents events1 = new CookieAuthenticationEvents();
                events1.OnValidatePrincipal = new Func<CookieValidatePrincipalContext, Task>(SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>);
                o.Events = events1;
            }).AddCookie(IdentityConstants.TwoFactorUserIdScheme, delegate (CookieAuthenticationOptions o) {
                o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
            });

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddMvc()
                .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

            services.AddSignalR()
                .AddJsonProtocol(options => { options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddCors(options => options.UseConfiguredCors(Configuration.GetSection("CorsPolicy")));

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SEER API V1");
            });

            app.UseForwardedHeaders();
            app.UseAuthentication();
            app.UseCors("default");
            app.UseHttpsRedirection();
            
            if(env.IsDevelopment())
            {
                // localhost chrome cookies cause "correlation" errors with identity
                // this fixes that problem
                app.UseCookiePolicy(new CookiePolicyOptions()
                {
                    MinimumSameSitePolicy = SameSiteMode.Lax
                });
            }
            
            app.UseRouting();
            app.UseAuthorization();

            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "areas",
                    "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<AssessmentTimeHub>("/hubs/time");
                endpoints.MapHub<ExecutionHub>("/hubs/execution");
                endpoints.MapHub<METHub>("/hubs/mets");
                endpoints.MapHub<MeasureHub>("/hubs/measure");
                endpoints.MapHub<TasksHub>("/hubs/tasks");

                endpoints.MapRazorPages();
            });
        }
    }
}