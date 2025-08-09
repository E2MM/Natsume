using NetCord;
using NetCord.Gateway;

namespace Natsume.NetCord.NatsumeNetCordModules;

public class NatsumeListeningContext(User natsumeDiscordUser, Message message, Channel channel)
{
    public string ContactName { get; } = message.Author.GetName();
    public User NatsumeDiscordUser { get; } = natsumeDiscordUser;
    public Message Message { get; } = message;
    public int MessageLength { get; } = message.Content.Length;
    public bool IsOwnMessage { get; init; } = message.Author == natsumeDiscordUser;
    public bool IsNatsumeTagged { get; } = message.MentionedUsers.Contains(natsumeDiscordUser);

    public bool IsNatsumeExplicitlyTagged { get; } =
        message.MentionedUsers.Contains(natsumeDiscordUser)
        && message.ReferencedMessage?.Author != natsumeDiscordUser;

    public bool IsEveryoneTagged { get; } = message.MentionEveryone;
    public bool IsDirectMessage { get; } = message.Channel is DMChannel;
    public Channel Channel { get; } = channel;
}