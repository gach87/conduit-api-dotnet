using Microsoft.EntityFrameworkCore;

namespace Conduit.Models
{
    public class ConduitContext : DbContext
    {
        public ConduitContext(DbContextOptions<ConduitContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<User> Users { get; set; }
    }
}