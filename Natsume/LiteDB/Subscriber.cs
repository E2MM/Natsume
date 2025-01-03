namespace Natsume.LiteDB;

public class Subscriber
{
    public ulong Id { get; set; }
    public bool ActiveSubscription { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal TotalBalanceCharged { get; set; }
    public DateTime? LastBalanceCharge { get; set; }
    public ulong InputTokensConsumed { get; set; }
    public ulong OutputTokensConsumed { get; set; }
    public DateTime? LastInvocation { get; set; }
    public uint TotalInvocations { get; set; }

    public Subscriber AddBalance(decimal amount)
    {
        CurrentBalance += amount;
        TotalBalanceCharged += amount;
        LastBalanceCharge = DateTime.Now;
        return this;
    }

    public Subscriber ConsumeBalance(int inputTokens, int outputTokens, decimal cost)
    {
        InputTokensConsumed += (ulong)inputTokens;
        OutputTokensConsumed += (ulong)outputTokens;
        LastInvocation = DateTime.Now;
        TotalInvocations++;
        CurrentBalance -= cost;
        return this;
    }
}