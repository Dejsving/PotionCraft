using Microsoft.EntityFrameworkCore;
using PotionCraft.Repository;
using PotionCraft.Repository.Abstraction;
using PotionCraft.Repository.Repositories;

namespace PotionCraft
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<PotionCraftDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=potioncraft.db";
                options.UseSqlite(connectionString);
            });

            builder.Services.AddScoped<IPlayerCharacterRepository, PlayerCharacterRepository>();
            builder.Services.AddScoped<IHerbRepository, HerbRepository>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PotionCraftDbContext>();
                dbContext.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
