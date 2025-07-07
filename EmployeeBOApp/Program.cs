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
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using EmployeeBOApp;
using EmployeeBOApp.Business.Implementations;
using EmployeeBOApp.BusinessLayer.Interfaces;
using EmployeeBOApp.BusinessLayer.Services;
using EmployeeBOApp.BussinessLayer.Interfaces;
using EmployeeBOApp.Data;
using EmployeeBOApp.Repositories; // Assuming your implementation class is here
using EmployeeBOApp.Repositories.Implementations;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IBGVRepository, BGVRepository>();
builder.Services.AddScoped<IBGVService, BGVService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IBGVViewRepository, BGVViewRepository>();
builder.Services.AddScoped<IBGVViewService, BGVViewService>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();



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
