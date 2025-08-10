namespace Natsume.Persistence.Contact;

public class NatsumeContact
{
    public ulong DiscordId { get; private set; }
    public string DiscordNickname { get; private set; } = string.Empty;
    public bool IsFriend { get; private set; }
    public decimal Friendship { get; }
    public decimal TimeFriendship { get; private set; }
    public decimal MessageFriendship { get; }
    public decimal CurrentFavor { get; private set; }
    public decimal MaximumFavor { get; }
    public decimal TotalFavorExpended { get; private set; }
    public decimal DailyAverageFavorExpended { get; }
    public ulong TotalInteractions { get; private set; }
    public DateTime? LastInteraction { get; private set; }
    public DateTime MetOn { get; private set; }
    public DateTime? LastBondUp { get; private set; }

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

    public NatsumeContact ExpendFavorForFriendship(decimal favorCost)
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
        var onePercentMissing = (MaximumFavor - CurrentFavor) / 100m;
        CurrentFavor += 0.33m * onePercentMissing;
        TimeFriendship += 3 / 65536m;
        LastBondUp = DateTime.Now;
        return this;
    }
}