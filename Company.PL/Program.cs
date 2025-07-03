using Company.Ali.BLL;
using Company.Ali.BLL.Interfaces;
using Company.Ali.BLL.Repositories;
using Company.Ali.DAL.Data.Contexts;
using Company.Ali.DAL.Models;
using Company.Ali.PL.Helper;
using Company.Ali.PL.Mapping;
using Company.Ali.PL.Services;
using Company.Ali.PL.Settings;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Company.Ali.PL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            
            //builder.Services.AddScoped<IDepartmentRepository,DepartmentRepository>(); // Allow DI For  DepartmentRepository
            //builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


            //builder.Services.AddScoped(); // Create Object Life Time Per Request - UnReachable Object
            //builder.Services.AddTransient(); // Create Object Life Time Per Operation
            //builder.Services.AddSingleton(); // Create Object Life Time Per App

            builder.Services.AddScoped<IScopedService, ScopedService>(); // 
            builder.Services.AddTransient<ITransentService, TransentService>();
            builder.Services.AddSingleton<ISingletonService, SingletonService>();


            builder.Services.AddDbContext<CompanyDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            } ); // Allow DI For CompanyDbContext

            //builder.Services.AddAutoMapper(typeof(EmployeeProfile));
            builder.Services.AddAutoMapper(M => M.AddProfile(new EmployeeProfile()));


            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

            builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection(nameof(TwilioSettings)));

            builder.Services.AddScoped<IMailService, MailService>();

            builder.Services.AddScoped<ITwilioService, TwilioService>();


            builder.Services.AddIdentity<AppUser, IdentityRole>()
                            .AddEntityFrameworkStores<CompanyDbContext>()
                            .AddDefaultTokenProviders();


            builder.Services.ConfigureApplicationCookie(config =>
            {
                config.LoginPath = "/Account/SignIn";

            }
            );

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            
                }
            ).AddGoogle( o => {
                o.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
