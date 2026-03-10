using Microsoft.EntityFrameworkCore;

using PotionCraft.Contracts;
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
    }
}
