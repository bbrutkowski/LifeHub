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
        public DbSet<FinanceTransaction> FinanceTransactions { get; set; } = null!;
        public DbSet<FinanceBudget> FinanceBudgets { get; set; } = null!;

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

                b.Property(u => u.AvatarUrl)
                    .HasMaxLength(512);

                b.Property(u => u.Timezone).HasMaxLength(100);
                b.Property(u => u.City).HasMaxLength(100);
                b.Property(u => u.Currency).HasMaxLength(3);
                b.Property(u => u.DateFormat).HasMaxLength(20);
                b.Property(u => u.WeekStartsOn).HasMaxLength(10);
                b.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);

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

            modelBuilder.Entity<FinanceTransaction>(b =>
            {
                b.ToTable("FinanceTransactions");
                b.HasKey(x => x.Id);
                b.Property(x => x.Type).IsRequired().HasMaxLength(10);
                b.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
                b.Property(x => x.Category).IsRequired().HasMaxLength(32);
                b.Property(x => x.Note).HasMaxLength(500);
                b.Property(x => x.Date).HasColumnType("date").IsRequired();
                b.HasIndex(x => new { x.UserId, x.Date });
                b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FinanceBudget>(b =>
            {
                b.ToTable("FinanceBudgets");
                b.HasKey(x => new { x.UserId, x.Month, x.Category });
                b.Property(x => x.Month).HasColumnType("date");
                b.Property(x => x.Category).HasMaxLength(32).IsRequired();
                b.Property(x => x.LimitAmount).HasPrecision(18, 2).IsRequired();
                b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
