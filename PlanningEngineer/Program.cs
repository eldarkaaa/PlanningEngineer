using PlanningEngineerApplication.Data;
using PlanningEngineerApplication.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSession(); 

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Buildings}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    await CreateRolesAndAdmin(context);
}

app.Run();

async Task CreateRolesAndAdmin(ApplicationDbContext context)
{
    if (!context.Roles.Any())
    {
        context.Roles.AddRange(new Role { Name = "Admin" }, new Role { Name = "Moderator" }, new Role { Name = "User" });
        await context.SaveChangesAsync();
    }

    if (!context.Users.Any())
    {
        var adminUser = new User
        {
            Email = "admin@admin.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Salt = BCrypt.Net.BCrypt.GenerateSalt(),
            RoleId = context.Roles.First(r => r.Name == "Admin").Id
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
