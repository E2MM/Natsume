namespace Natsume.LiteDB;

public class NatsumeContact
{
    public ulong Id { get; private set; }
    public bool IsNatsumeFriend { get; private set; }
    public string Nickname { get; private set; } = "User";
    public decimal CurrentFriendship { get; private set; }
    public decimal TimeFriendship { get; private set; }
    public decimal ActivityFriendship { get; private set; }
    public decimal MessageFriendship { get; private set; }
    public decimal MaximumFriendship => 1M + TimeFriendship + ActivityFriendship + MessageFriendship;
    public decimal TotalFriendshipExpended { get; private set; }
    public decimal DailyFriendshipExpended => TotalFriendshipExpended / (decimal)(DateTime.Now - FriendsSince).TotalDays;
    public DateTime? LastMessageOn { get; private set; }
    public ulong MessageCount { get; private set; }
    public DateTime FriendsSince { get; private set; }

    public NatsumeContact()
    {
    }

    public NatsumeContact(ulong id, string nickname)
    {
        Id = id;
        IsNatsumeFriend = true;
        Nickname = nickname;
        CurrentFriendship = 0.25M;
        TimeFriendship = 0M;
        ActivityFriendship = 0M;
        MessageFriendship = 0M;
        LastMessageOn = null;
        TotalFriendshipExpended = 0M;
        MessageCount = 0;
        FriendsSince = DateTime.Now;
    }

    public NatsumeContact AwardFriendship(decimal amount)
    {
        CurrentFriendship += amount;
        return this;
    }

    public NatsumeContact BurnFriendship(decimal amount)
    {
        CurrentFriendship -= amount;
        return this;
    }

    public NatsumeContact AskAFavorForFriendship(decimal friendshipCost)
    {
        CurrentFriendship -= friendshipCost;
        TotalFriendshipExpended += friendshipCost;
        MessageCount++;
        MessageFriendship = (decimal)Math.Pow(Math.Log(MessageCount, Math.E), 2) / 100M;
        LastMessageOn = DateTime.Now;
        return this;
    }

    public NatsumeContact Befriend()
    {
        IsNatsumeFriend = true;
        return this;
    }

    public NatsumeContact Unfriend()
    {
        IsNatsumeFriend = false;
        return this;
    }

    public NatsumeContact BondUp()
    {
        var onePerThousandMissing = (MaximumFriendship - CurrentFriendship) / 1000M;
        CurrentFriendship += onePerThousandMissing;
        TimeFriendship += 1M / 256M / 256M;
        return this;
    }
}