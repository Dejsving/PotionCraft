using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using PotionCraft.Contracts;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Models;

namespace PotionCraft.Repository
{
    public class PotionCraftDbContext : DbContext
    {
        public PotionCraftDbContext(DbContextOptions<PotionCraftDbContext> options)
            : base(options)
        {
        }

        public DbSet<PlayerCharacter> PlayerCharacters { get; set; }
        
        public DbSet<Herb> Herbs { get; set; }

        /// <summary>
        /// Сохраняет изменения, предварительно обновляя токены конкурентности.
        /// </summary>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateRowVersions();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// Асинхронно сохраняет изменения, предварительно обновляя токены конкурентности.
        /// </summary>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateRowVersions();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// Обновляет RowVersion для добавленных и изменённых сущностей PlayerCharacter.
        /// Необходимо для SQLite, который не поддерживает автоматический rowversion.
        /// </summary>
        private void UpdateRowVersions()
        {
            foreach (var entry in ChangeTracker.Entries<PlayerCharacter>()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added))
            {
                entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
            }
        }

        /// <summary>
        /// Создаёт ValueComparer для словаря Dictionary&lt;Guid, TValue&gt; на основе JSON-сериализации.
        /// </summary>
        private static ValueComparer<Dictionary<Guid, TValue>> CreateDictionaryComparer<TValue>(JsonSerializerOptions jsonOptions)
        {
            return new ValueComparer<Dictionary<Guid, TValue>>(
                (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
                c => JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<Guid, TValue>>(
                    JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var jsonOptions = new JsonSerializerOptions();

            modelBuilder.Entity<PlayerCharacter>()
                .HasIndex(pc => pc.Name)
                .IsUnique();

            modelBuilder.Entity<PlayerCharacter>()
                .Property(pc => pc.RowVersion)
                .IsConcurrencyToken();

            modelBuilder.Entity<PlayerCharacter>()
                .OwnsOne(pc => pc.AlchemistTool);

            modelBuilder.Entity<PlayerCharacter>()
                .OwnsOne(pc => pc.HerbalismTool);

            modelBuilder.Entity<PlayerCharacter>()
                .OwnsOne(pc => pc.PoisonerTool);

            var herbsComparer = CreateDictionaryComparer<GatheringResult>(jsonOptions);
            var potionBagComparer = CreateDictionaryComparer<PotionBagItem>(jsonOptions);

            modelBuilder.Entity<PlayerCharacter>()
                .OwnsOne(pc => pc.Bag, bag =>
                {
                    bag.Property(b => b.Herbs)
                        .HasConversion(
                            v => JsonSerializer.Serialize(v, jsonOptions),
                            v => JsonSerializer.Deserialize<Dictionary<Guid, GatheringResult>>(v, jsonOptions) ?? new Dictionary<Guid, GatheringResult>())
                        .Metadata.SetValueComparer(herbsComparer);

                    bag.Property(b => b.Potions)
                        .HasConversion(
                            v => JsonSerializer.Serialize(v, jsonOptions),
                            v => JsonSerializer.Deserialize<Dictionary<Guid, PotionBagItem>>(v, jsonOptions) ?? new Dictionary<Guid, PotionBagItem>())
                        .Metadata.SetValueComparer(potionBagComparer);

                    bag.Property(b => b.Poisons)
                        .HasConversion(
                            v => JsonSerializer.Serialize(v, jsonOptions),
                            v => JsonSerializer.Deserialize<Dictionary<Guid, PotionBagItem>>(v, jsonOptions) ?? new Dictionary<Guid, PotionBagItem>())
                        .Metadata.SetValueComparer(potionBagComparer);
                });

            modelBuilder.Entity<Herb>()
                .Property(h => h.Habitats)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<TerrainEnum, int>>(v, jsonOptions) as IReadOnlyDictionary<TerrainEnum, int> ?? new Dictionary<TerrainEnum, int>());
        }
    }
}

