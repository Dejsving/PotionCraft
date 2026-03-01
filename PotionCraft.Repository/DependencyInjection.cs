using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PotionCraft.Repository.Interfaces;
using PotionCraft.Repository.Repositories;

namespace PotionCraft.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PotionCraftDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IHerbRepository, HerbRepository>();

        return services;
    }
}
