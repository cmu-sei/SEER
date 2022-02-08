/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH
Copyright 2021 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM21-0384
*/

using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
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
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            _isDevelopment = env.IsDevelopment();
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            services
                .AddApplicationData(_conf.Database.Provider, _conf.Database.ConnectionString)
                .AddApplicationServices();

            services.AddIdentityCore<User>()
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
                            $"Copyright {DateTime.Now.Year} Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms"
                    }
                });
            });

            services.AddSingleton<IFileProvider>(
                new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "App_Data")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie(IdentityConstants.ApplicationScheme, o =>
                {
                    o.LoginPath = new PathString("/Account/Login");
                    o.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                    };
                })
                .AddCookie(IdentityConstants.ExternalScheme, o =>
                {
                    o.Cookie.Name = IdentityConstants.ExternalScheme;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                })
                .AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
                {
                    o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
                    o.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
                    };
                })
                .AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
                {
                    o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.Cookie.Name = "seer_identity";
                    options.Cookie.HttpOnly = true;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    // from Jeff
                    // All the pcke stuff should be handled for you, so just a matter of specifying it.
                    // (I guess the caveat is that the Identity Provider supports pcke, which most do, including ours.)
                    options.RequireHttpsMetadata = !_isDevelopment && options.RequireHttpsMetadata;
                    options.UsePkce = true;

                    options.Authority = _conf.AuthenticationAuthority;
                    options.RequireHttpsMetadata = _conf.RequireHttpsMetadata;
                    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                    options.SaveTokens = true;
                    // this makes another trip to the server to fetch the information that is essentially in the token
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.ClientId = _conf.ClientId;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    options.Scope.Add("profile");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role
                    };

                    //Additional config snipped
                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenResponseReceived = async ctx => { await UserService.CreateAccountAsync(ctx, _conf); },
                        OnRedirectToIdentityProvider = context =>
                        {
                            context.ProtocolMessage.RedirectUri = $"{_conf.Host}/signin-oidc";
                            return Task.FromResult(0);
                        },
                        OnRemoteFailure = context =>
                        {
                            if (context.Failure != null)
                                context.Response.Redirect($"/account/login?e=remote_failure&m={context.Failure.Message}");
                            context.HandleResponse();
                            return Task.FromResult(0);
                        },
                        OnAuthenticationFailed = context =>
                        {
                            context.Response.Redirect($"/account/login?e=auth_failure&m={context.Exception.Message}");
                            context.HandleResponse();
                            return Task.FromResult(0);
                        }
                    };
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
                c.OAuthClientId(_conf.ClientId);
                c.OAuthClientSecret(_conf.ClientSecret);
                c.OAuthAppName(_conf.ClientName);
            });

            app.UseForwardedHeaders();
            app.UseAuthentication();
            app.UseCors("default");
            //app.UseHttpsRedirection();
            
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

                endpoints.MapHub<AdministrationHub>("/hubs/administration");
                endpoints.MapHub<AssessmentTimeHub>("/hubs/time");
                endpoints.MapHub<ExecutionHub>("/hubs/execution");
                endpoints.MapHub<METHub>("/hubs/mets");
                endpoints.MapHub<MeasureHub>("/hubs/measure");
                endpoints.MapHub<MouseTrackingHub>("/hubs/mouse");
                endpoints.MapHub<QuizHub>("/hubs/quizzes");
                endpoints.MapHub<TasksHub>("/hubs/tasks");

                endpoints.MapRazorPages();
            });
        }
    }
}