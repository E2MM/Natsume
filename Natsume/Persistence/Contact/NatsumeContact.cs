namespace Natsume.Persistence.Contact;

public class NatsumeContact
{
    public ulong DiscordId { get; private set; }
    public string DiscordNickname { get; private set; } = string.Empty;
    public bool IsFriend { get; private set; }
    public decimal CurrentFavor { get; private set; }
    public decimal TimeFriendship { get; private set; }
    public decimal MessageFriendship => (decimal)Math.Pow(Math.Log(TotalInteractions, Math.E), 2) / 100M;
    public decimal Friendship => 100 * TotalFavorExpended * (1M + TimeFriendship + MessageFriendship);
    public decimal MaximumFavor => 1M + TimeFriendship + MessageFriendship;
    public decimal TotalFavorExpended { get; private set; }
    public decimal DailyAverageFavorExpended => TotalFavorExpended / (decimal)(DateTime.Now - MetOn).TotalDays;
    public DateTime? LastInteraction { get; private set; }
    public ulong TotalInteractions { get; private set; }
    public DateTime MetOn { get; private set; }

    private NatsumeContact()
    {
    }

    public NatsumeContact(ulong discordId, string discordNickname, bool isFriend = true)
    {
        DiscordId = discordId;
        DiscordNickname = discordNickname;
        IsFriend = isFriend;
        CurrentFavor = 0.25M;
        TimeFriendship = 0M;
        LastInteraction = null;
        TotalFavorExpended = 0M;
        TotalInteractions = 0;
        MetOn = DateTime.Now;
    }

    public NatsumeContact AwardFavor(decimal amount)
    {
        CurrentFavor += amount;
        return this;
    }

    public NatsumeContact ConsumeFavor(decimal amount)
    {
        CurrentFavor -= amount;
        return this;
    }

    public NatsumeContact Interact()
    {
        LastInteraction = DateTime.Now;
        TotalInteractions += 1;
        return this;
    }

    public NatsumeContact AskAFavorForFriendship(decimal favorCost)
    {
        CurrentFavor -= favorCost;
        TotalFavorExpended += favorCost;
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
        var onePerThousandMissing = (MaximumFavor - CurrentFavor) / 1000M;
        CurrentFavor += onePerThousandMissing;
        TimeFriendship += 1M / 256M / 256M;
        return this;
    }
}