using LifeHub.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace LifeHub.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshTokenSession> RefreshTokenSessions { get; set; } = null!;

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

            modelBuilder.Entity<RefreshTokenSession>(b =>
            {
                b.ToTable("RefreshTokenSessions");

                b.HasKey(x => x.Id);

                b.Property(x => x.TokenHash)
                    .IsRequired()
                    .HasMaxLength(64);

                b.Property(x => x.ExpiresAt)
                    .IsRequired();

                b.Property(x => x.CreatedAt)
                    .IsRequired();

                b.Property(x => x.ReplacedByTokenHash)
                    .HasMaxLength(64);

                b.HasIndex(x => x.TokenHash)
                    .IsUnique();

                b.HasIndex(x => x.UserId);

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
