using LifeHub.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace LifeHub.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");

                b.HasKey(u => u.Id);

                b.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(512);

                b.Property(u => u.CreatedAt)
                    .IsRequired();

                b.HasIndex(u => u.Username).IsUnique();
                b.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
