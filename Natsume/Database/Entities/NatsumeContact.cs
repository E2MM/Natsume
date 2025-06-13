using NetCord;

namespace Natsume.Database.Entities;

public class NatsumeContact
{
    public ulong DiscordId { get; private set; }
    public bool IsFriend { get; private set; }
    public string Nickname { get; private set; } = string.Empty;
    public decimal AvailableFavor { get; private set; }
    public decimal TimeFriendship { get; private set; }
    public decimal ActivityFriendship { get; private set; }
    public decimal MessageFriendship { get; private set; }
    public decimal Friendship => 1M + TimeFriendship + ActivityFriendship + MessageFriendship;
    public decimal TotalFavorExpended { get; private set; }
    public decimal DailyAverageFavorExpended => TotalFavorExpended / (decimal)(DateTime.Now - FriendsSince).TotalDays;
    public DateTime? LastMessageOn { get; private set; }
    public ulong MessageCount { get; private set; }
    public DateTime FriendsSince { get; private set; }

    private NatsumeContact()
    {
    }

    public NatsumeContact(ulong discordId, string nickname)
    {
        DiscordId = discordId;
        IsFriend = true;
        Nickname = nickname;
        AvailableFavor = 0.25M;
        TimeFriendship = 0M;
        ActivityFriendship = 0M;
        MessageFriendship = 0M;
        LastMessageOn = null;
        TotalFavorExpended = 0M;
        MessageCount = 0;
        FriendsSince = DateTime.Now;
    }

    public NatsumeContact AwardFriendship(decimal amount)
    {
        AvailableFavor += amount;
        return this;
    }

    public NatsumeContact BurnFriendship(decimal amount)
    {
        AvailableFavor -= amount;
        return this;
    }

    public NatsumeContact AskAFavorForFriendship(decimal friendshipCost)
    {
        AvailableFavor -= friendshipCost;
        TotalFavorExpended += friendshipCost;
        MessageCount++;
        MessageFriendship = (decimal)Math.Pow(Math.Log(MessageCount, Math.E), 2) / 100M;
        LastMessageOn = DateTime.Now;
        return this;
    }

    public NatsumeContact Befriend()
    {
        IsFriend = true;
        return this;
    }

    public NatsumeContact Unfriend()
    {
        IsFriend = false;
        return this;
    }

    public NatsumeContact BondUp()
    {
        var onePerThousandMissing = (Friendship - AvailableFavor) / 1000M;
        AvailableFavor += onePerThousandMissing;
        TimeFriendship += 1M / 256M / 256M;
        return this;
    }
}