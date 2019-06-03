using Esdi.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System;
using UserManagement.Entities;
using UserManagement.Services;
using UserManagement.Services.Helpers;
using AutoMapper;

namespace UserManagement
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // First create configuration service, which may be used during the rest of the configuration
            services.AddSingleton(Configuration);

            services.Configure<JwtOptions>(Configuration.GetSection("JwtOptions"));

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
                    config.SignIn.RequireConfirmedEmail = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            var jwtOptions = Configuration.GetSection(nameof(JwtOptions));
            string test = jwtOptions[nameof(JwtOptions.ValidIssuer)];
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions[nameof(JwtOptions.ValidIssuer)],

                ValidateAudience = true,
                ValidAudience = jwtOptions[nameof(JwtOptions.ValidAudience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Convert.FromBase64String(jwtOptions[nameof(JwtOptions.SecurityKey)])
                    ),

                RequireExpirationTime = true,
                ValidateLifetime = true

                //ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.ClaimsIssuer = jwtOptions[nameof(JwtOptions.ValidIssuer)];
                opt.SaveToken = true;
                opt.TokenValidationParameters = tokenValidationParameters;
            });

            string smtpServer = Configuration.GetSection("Emailing:SmtpServer").Value;
            int.TryParse(Configuration.GetSection("Emailing:SmtpPort").Value, out int port);
            string username = Configuration.GetSection("Emailing:Username").Value;
            string password = Configuration.GetSection("Emailing:Password").Value;

            services.AddSingleton<IEmailSender, EmailSender>(
                emailSender => new EmailSender(smtpServer, port, username, password));

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddSingleton<IStringLocalizer>((ctx) =>
            {
                IStringLocalizerFactory factory = ctx.GetService<IStringLocalizerFactory>();
                return factory.Create(typeof(SharedResources));
            });

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<IHelperService, HelperService>();

            services.AddAutoMapper();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
