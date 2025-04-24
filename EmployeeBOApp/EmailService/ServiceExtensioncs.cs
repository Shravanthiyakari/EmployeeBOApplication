using EmployeeBOApp.EmailServices;
using EmployeeBOApp.Models;


namespace EmployeeBOApp
{
    public static class ServiceExtensions
    {
        public static void ConfigureEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailService, EmailService>();
        }
    }
}