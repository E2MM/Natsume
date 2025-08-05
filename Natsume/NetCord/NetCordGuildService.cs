namespace Natsume.NetCord;

public class NetCordGuildService(ulong guildId, ulong mainChannelId)
{
    public ulong GuildId { get; } = guildId;
    public ulong MainChannelId { get; } = mainChannelId;
}