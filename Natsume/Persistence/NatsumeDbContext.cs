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

            entity.Property(x => x.MessageFriendship)
                .HasComputedColumnSql(
                    sql: "(LN(1 + [TotalInteractions]) * LN(1 + [TotalInteractions]) / 100.0)",
                    stored: false
                );

            entity.Property(x => x.Friendship)
                .HasComputedColumnSql(
                    sql: "(100 * [TotalFavorExpended] * (1 + [TimeFriendship] + [MessageFriendship]))",
                    stored: false
                );

            entity.Property(x => x.MaximumFavor)
                .HasComputedColumnSql(
                    sql: "(1 + [TimeFriendship] + [MessageFriendship])",
                    stored: false
                );

            entity.Property(x => x.DailyAverageFavorExpended)
                .HasComputedColumnSql(
                    sql: """
                         CASE WHEN (JULIANDAY(COALESCE([LastInteraction], [MetOn])) - JULIANDAY([MetOn])) > 0 
                         THEN [TotalFavorExpended] / (JULIANDAY([LastInteraction]) - JULIANDAY([MetOn])) 
                         ELSE 0 END
                         """,
                    stored: false
                );
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