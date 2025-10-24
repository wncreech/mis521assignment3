using Betterboxd.Data;
using Microsoft.EntityFrameworkCore;

namespace Betterboxd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Get connection string for Azure SQL
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Use SQL Server instead of SQLite
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            var app = builder.Build();

            // Configure middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // better for dev
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Betterboxd API V1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            // MVC default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
