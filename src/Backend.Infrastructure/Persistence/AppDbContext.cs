using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Backend.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
        public DbSet<ConversionHistory> ConversionHistories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ConversionHistory>(b =>
            {
                b.HasKey(x => x.ConversionId);
                b.Property(x => x.FromCurrency).HasMaxLength(3).IsRequired();
                b.Property(x => x.ToCurrency).HasMaxLength(3).IsRequired();
                b.Property(x => x.ExchangeRate).HasColumnType("decimal(18,8)");
                b.Property(x => x.FromAmount).HasColumnType("decimal(18,6)");
                b.Property(x => x.ToAmount).HasColumnType("decimal(18,6)");
            });
        }
    }
}