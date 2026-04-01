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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var jsonOptions = new JsonSerializerOptions();

            modelBuilder.Entity<PlayerCharacter>()
                .HasIndex(pc => pc.Name)
                .IsUnique();

            modelBuilder.Entity<PlayerCharacter>()
                .OwnsOne(pc => pc.AlchemistTool);

            modelBuilder.Entity<PlayerCharacter>()
                .OwnsOne(pc => pc.HerbalismTool);

            modelBuilder.Entity<PlayerCharacter>()
                .OwnsOne(pc => pc.PoisonerTool);

            var herbsComparer = new ValueComparer<Dictionary<Guid, GatheringResult>>(
                (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
                c => JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<Guid, GatheringResult>>(
                    JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!);

            var potionBagComparer = new ValueComparer<Dictionary<Guid, PotionBagItem>>(
                (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
                c => JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<Guid, PotionBagItem>>(
                    JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!);

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

