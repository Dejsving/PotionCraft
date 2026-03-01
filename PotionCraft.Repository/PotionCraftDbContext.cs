using Microsoft.EntityFrameworkCore;
using PotionCraft.Contracts.Models;

namespace PotionCraft.Repository;

public class PotionCraftDbContext : DbContext
{
    public PotionCraftDbContext(DbContextOptions<PotionCraftDbContext> options) : base(options)
    {
    }

    public DbSet<Herb> Herbs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка таблицы ингридиентов (трав)
        modelBuilder.Entity<Herb>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(1000);

            // Перечисление Habitats (коллекция Terrain) в SQLite будет сохранено как JSON строка
            // Эта фича поддерживается в EF Core начиная с 8.0 "из коробки"
            
            // Настройка таблицы-справочника компонентов, связанных с травой
            entity.OwnsMany(e => e.Components, component =>
            {
                component.ToTable("HerbComponents");
                component.Property<int>("Id"); // Создаем теневой ключ для хранения в отдельной таблице
                component.HasKey("Id");

                component.Property(c => c.EffectDescription).HasMaxLength(500);
                component.Property(c => c.RequiredTool).HasMaxLength(100);
            });
        });
    }
}
