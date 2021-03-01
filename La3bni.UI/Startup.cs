using La3bni.UI.Payment;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;
using Repository;
using Stripe;
using System;

namespace La3bni.UI
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<La3bniContext>(options => options.UseSqlServer(configuration.GetConnectionString("La3bniCon")));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IEmailRepository, EmailRepository>();

            services.AddScoped<ImageManager, ImageManager>();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<La3bniContext>()
            .AddDefaultTokenProviders();
            //LifeSpan for our tokens
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            opt.TokenLifespan = TimeSpan.FromHours(2));
            services.AddAuthentication()
                .AddGoogle(opt =>
                {
                    opt.ClientId = "370639406913-mvjdn4trffvdo9sciptf26ve3qqaumpv.apps.googleusercontent.com";
                    opt.ClientSecret = "2nLuKM6LRo09WEiONbE9wcpa";
                })
                .AddFacebook(opt =>
                {
                    opt.ClientId = "691976331484726";
                    opt.ClientSecret = "7ee0514ba330aaf2cf97348fbaf11d86";
                });
            services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/{0}");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            StripeConfiguration.ApiKey = configuration.GetSection("Stripe")["SecretKey"];

            app.UseStaticFiles();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapDefaultControllerRoute();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}