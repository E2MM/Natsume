namespace Natsume.Persistence.Reminder;

public class NatsumeReminder
{
    public int Id { get; private set; }
    public ulong DiscordChannelId { get; private set; }
    public ulong DiscordMessageId { get; private set; }
    public ulong DiscordUserId { get; private set; }
    public DateTime RemindMeAt { get; private set; }
    public string ReminderText { get; private set; } = string.Empty;

    private NatsumeReminder()
    {
    }

    public NatsumeReminder(
        ulong discordChannelId,
        ulong discordMessageId,
        ulong discordUserId,
        DateTime remindMeAt,
        string reminderText
    )
    {
        DiscordChannelId = discordChannelId;
        DiscordMessageId = discordMessageId;
        DiscordUserId = discordUserId;
        RemindMeAt = remindMeAt;
        ReminderText = reminderText;
    }
}