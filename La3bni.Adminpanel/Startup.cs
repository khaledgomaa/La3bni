using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;
using Repository;
using Repository.IBookingRepository;
using System.IO;

namespace La3bni.Adminpanel
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration _configurable)
        {
            configuration = _configurable;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddDbContext<La3bniContext>(options => options.UseSqlServer(configuration.GetConnectionString("La3bniCon")));

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Images")))
            {
                Directory.CreateDirectory("Images");
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",

                    pattern: "{area=Admin}/{controller=Dashboard}/{action=Index}/{id?}"

                    );

                endpoints.MapControllerRoute(
                name: "default",
                 pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}