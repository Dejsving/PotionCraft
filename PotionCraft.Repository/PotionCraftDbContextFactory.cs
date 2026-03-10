using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PotionCraft.Repository
{
    public class PotionCraftDbContextFactory : IDesignTimeDbContextFactory<PotionCraftDbContext>
    {
        public PotionCraftDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PotionCraftDbContext>();
            optionsBuilder.UseSqlite("Data Source=potioncraft.db");
            return new PotionCraftDbContext(optionsBuilder.Options);
        }
    }
}