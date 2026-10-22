using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PotionCraft.Contracts.Identity;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Services;
using PotionCraft.InitData;
using PotionCraft.Repository;
using PotionCraft.Repository.Abstraction;
using PotionCraft.Repository.Repositories;
using PotionCraft.Server.Services.Email;
using PotionCraft.Server.Services.Gathering;
using PotionCraft.Server.Services.Identity;

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

            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(IdentityConfiguration.ConfigureIdentityOptions)
                .AddEntityFrameworkStores<PotionCraftDbContext>()
                .AddDefaultTokenProviders()
                .AddErrorDescriber<RussianIdentityErrorDescriber>();

            builder.Services.ConfigureApplicationCookie(options =>
                IdentityConfiguration.ConfigureCookie(options, builder.Environment.IsDevelopment()));

            // Роли в cookie перечитываются из БД раз в минуту, чтобы смена роли админом применялась быстро.
            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
                options.ValidationInterval = TimeSpan.FromMinutes(1));

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddScoped<IPlayerCharacterRepository, PlayerCharacterRepository>();
            builder.Services.AddScoped<IHerbRepository, HerbRepository>();
            builder.Services.AddSingleton<IDiceRoller, DiceRoller>();
            builder.Services.AddSingleton<IPriceCalculator, HerbPriceCalculator>();
            builder.Services.AddSingleton<IInventoryGenerator, ShopInventoryGenerator>();
            builder.Services.AddSingleton<IPotionEffectResolver, PotionEffectResolver>();
            builder.Services.AddScoped<IGatheringService, GatheringService>();
            builder.Services.AddScoped<IdentitySeeder>();
            builder.Services.AddSingleton(TimeProvider.System);
            // Email:Provider = "Console" (письма в лог сервера) или "Smtp" (настоящая почта).
            builder.Services.AddEmailSender(builder.Configuration);
            builder.Services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ClientPolicy", policy =>
                {
                    policy.WithOrigins(
                            builder.Configuration["ClientOrigin"] ?? "https://localhost:7200",
                            "http://localhost:5200",
                            builder.Configuration["AllowedCorsOrigin"] ?? "http://localhost:5173",
                            "http://127.0.0.1:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
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

                var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
                await identitySeeder.SeedAsync();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors("ClientPolicy");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
