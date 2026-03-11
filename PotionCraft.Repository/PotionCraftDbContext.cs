using System.Text.Json;
using Microsoft.EntityFrameworkCore;

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

            modelBuilder.Entity<Herb>()
                .Property(h => h.AdditionalRule)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<TerrainEnum, int>>(v, jsonOptions) as IReadOnlyDictionary<TerrainEnum, int> ?? new Dictionary<TerrainEnum, int>());

            modelBuilder.Entity<Herb>()
                .Property(h => h.Habitats)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<TerrainEnum, int>>(v, jsonOptions) as IReadOnlyDictionary<TerrainEnum, int> ?? new Dictionary<TerrainEnum, int>());
        }
    }
}

