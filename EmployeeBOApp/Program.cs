//using EmployeeBOApp;
//using EmployeeBOApp.Data;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// Register your DbContext with connection string
//builder.Services.AddDbContext<EmployeeDatabaseContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Add Cookie Authentication
//builder.Services.AddDbContext<EmployeeDatabaseContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.ConfigureEmailService(builder.Configuration);
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Login/Login";
//        options.LogoutPath = "/Login/Logout";
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//    });

//// Add Session
//builder.Services.AddSession(options =>
//{
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

//var app = builder.Build();

//// Configure middleware
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Login/Login");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthentication();    // ✅ Must be before Authorization
//app.UseAuthorization();

//app.UseSession();           // ✅ Enable session

//// Map default route
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Login}/{action=Login}/{id?}");

//app.Run();
using EmployeeBOApp;
using EmployeeBOApp.Data;
using EmployeeBOApp.Repositories.Interfaces;
using EmployeeBOApp.Repositories; // Assuming your implementation class is here
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using EmployeeBOApp.Repositories.Implementations;
using EmployeeBOApp.BussinessLayer.Interfaces;
using EmployeeBOApp.Business.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register DbContext
builder.Services.AddDbContext<EmployeeDatabaseContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("EmployeeDatabaseDbConnectionString");

    options.UseMySql(connectionString, new MySqlServerVersion(new Version(11, 8, 2)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        ));
});

// Register custom services
builder.Services.ConfigureEmailService(builder.Configuration);
builder.Services.AddScoped<IAllocationRepository, AllocationRepository>();
builder.Services.AddScoped<IDeallocationRepository, DeallocationRepository>();
builder.Services.AddScoped<IReportingRepository, ReportingRepository>();
builder.Services.AddScoped<IViewRepository, ViewRepository>();
builder.Services.AddScoped<IAllocationService, AllocationService>();


// Configure authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.LogoutPath = "/Login/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// Configure session
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Login/Login");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
