using Microsoft.EntityFrameworkCore;
using Natsume.Persistence.Contact;
using Natsume.Persistence.Meeting;
using Natsume.Persistence.Reminder;

namespace Natsume.Persistence;

public class NatsumeDbContext(DbContextOptions<NatsumeDbContext> options) : DbContext(options)
{
    public DbSet<NatsumeContact> Contacts { get; set; } = null!;
    public DbSet<NatsumeReminder> Reminders { get; set; } = null!;
    public DbSet<NatsumeMeeting> Meetings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NatsumeContact>(entity =>
        {
            entity.HasKey(e => e.DiscordId);

            entity.Property(e => e.DiscordNickname)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.CurrentFavor)
                .HasPrecision(precision: 18, scale: 6);

            entity.Property(e => e.TimeFriendship)
                .HasPrecision(precision: 18, scale: 6);

            entity.Property(e => e.TotalFavorExpended)
                .HasPrecision(precision: 18, scale: 6);
        });

        modelBuilder.Entity<NatsumeReminder>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ReminderText)
                .IsRequired()
                .HasMaxLength(512);
        });

        modelBuilder.Entity<NatsumeMeeting>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.MeetingName)
                .IsRequired()
                .HasMaxLength(32);
        });
    }
}