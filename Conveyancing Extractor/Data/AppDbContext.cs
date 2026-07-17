using Microsoft.EntityFrameworkCore;

namespace Conveyancing_Extractor.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<SolicitorEntity> Solicitors => Set<SolicitorEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SolicitorEntity>(entity =>
            {
                // SolicitorId is a SHA-256 hash of address + source, so it is
                // globally unique per physical branch on its own.
                entity.HasIndex(e => e.SolicitorId)
                      .IsUnique();

                entity.HasIndex(e => e.Location);
            });
        }
    }
}
