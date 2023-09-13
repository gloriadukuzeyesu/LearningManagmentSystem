using LMS.Areas.Identity.Data;
using LMS.Models;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Configuration.AddJsonFile("db.json", true);

        var identityConnectionString = builder.Configuration["LMS:IdentityConnectionString"];
        builder.Services.AddDbContext<LMSIdentityDbContext>(options =>
            options.UseMySql(identityConnectionString, ServerVersion.AutoDetect(identityConnectionString)));


        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<LMSIdentityDbContext>();

        // Uncomment the below after scaffolding
        
        builder.Services.AddDbContext<LMSContext>( options =>
            options.UseMySql( builder.Configuration["LMS:LMSConnectionString"], ServerVersion.AutoDetect( builder.Configuration["LMS:LMSConnectionString"] ) ) );
        

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();


        builder.Services.Configure<IdentityOptions>(options =>
                {
                    // Default Password settings.
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 1;
                    options.Password.RequiredUniqueChars = 1;
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                });


        var app = builder.Build();
        app.UseAuthentication();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        //app.UseHttpLogging(); //uncomment this to get extra logging info (probably not super helpful)

        app.UseRouting();


        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();


        //adapted from https://codewithmukesh.com/blog/user-management-in-aspnet-core-mvc/
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            try
            {
                var context = services.GetRequiredService<LMSIdentityDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                await SeedRoles.SeedRolesAsync(userManager, roleManager);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
        }

        app.Run();
    }
}
