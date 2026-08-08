using Microsoft.EntityFrameworkCore;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Services;
using PotionCraft.InitData;
using PotionCraft.Repository;
using PotionCraft.Repository.Abstraction;
using PotionCraft.Repository.Repositories;
using PotionCraft.Server.Services.Gathering;

namespace PotionCraft.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddDbContext<PotionCraftDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=potioncraft.db";
                options.UseSqlite(connectionString);
            });

            builder.Services.AddScoped<IPlayerCharacterRepository, PlayerCharacterRepository>();
            builder.Services.AddScoped<IHerbRepository, HerbRepository>();
            builder.Services.AddSingleton<IDiceRoller, DiceRoller>();
            builder.Services.AddSingleton<IPriceCalculator, HerbPriceCalculator>();
            builder.Services.AddSingleton<IInventoryGenerator, ShopInventoryGenerator>();
            builder.Services.AddSingleton<IPotionEffectResolver, PotionEffectResolver>();
            builder.Services.AddScoped<IGatheringService, GatheringService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ClientPolicy", policy =>
                {
                    policy.WithOrigins(
                            builder.Configuration["ClientOrigin"] ?? "https://localhost:7200",
                            "http://localhost:5200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PotionCraftDbContext>();
                if (dbContext.Database.IsRelational())
                    dbContext.Database.Migrate();
                else
                    dbContext.Database.EnsureCreated();

                await HerbDataSeeder.SeedHerbsAsync(dbContext);
                await dbContext.PlayerCharacters
                    .Where(c => c.SelectedBy != null)
                    .ExecuteUpdateAsync(s => s.SetProperty(c => c.SelectedBy, (Guid?)null));
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors("ClientPolicy");
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
