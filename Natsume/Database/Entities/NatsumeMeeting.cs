namespace Natsume.Database.Entities;

public class NatsumeMeeting
{
    public int Id { get; private set; }
    public string MeetingName { get; private set; } = string.Empty;
    public bool IsRandomMeeting { get; private set; } = false;
    public DateTime CreatedAt { get; private set; }
    public ulong DiscordUserId { get; private set; }

    private NatsumeMeeting()
    {
    }

    internal NatsumeMeeting(string meetingName, ulong discordUserId, bool isRandomMeeting = false)
    {
        MeetingName = meetingName;
        IsRandomMeeting = isRandomMeeting;
        CreatedAt = DateTime.Now;
        DiscordUserId = discordUserId;
    }
}