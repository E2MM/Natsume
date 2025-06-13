using Microsoft.EntityFrameworkCore;
using Natsume.Database.Entities;

namespace Natsume.Database;

public class NatsumeDbContext(DbContextOptions<NatsumeDbContext> options) : DbContext(options)
{
    public DbSet<NatsumeContact> Contacts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NatsumeContact>(entity =>
        {
            entity.HasKey(e => e.DiscordId);

            entity.Property(e => e.Nickname)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.AvailableFavor)
                .HasPrecision(precision: 18, scale: 6);

            entity.Property(e => e.TimeFriendship)
                .HasPrecision(precision: 18, scale: 6);

            entity.Property(e => e.ActivityFriendship)
                .HasPrecision(precision: 18, scale: 6);

            entity.Property(e => e.MessageFriendship)
                .HasPrecision(precision: 18, scale: 6);

            entity.Property(e => e.TotalFavorExpended)
                .HasPrecision(precision: 18, scale: 6);
        });
    }
}