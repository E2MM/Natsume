using Natsume.NetCord.NatsumeAI;
using Natsume.Services;
using NetCord;
using NetCord.Gateway;

namespace Natsume.NetCord.NatsumeNetCordModules;

internal class NatsumeListeningContext(NatsumeAi natsumeAi, Message message, User userNatsume)
{
    public string ContactName { get; } = message.Author.GetName();
    public NatsumeAi NatsumeAi { get; } = natsumeAi;
    public User Natsume { get; } = userNatsume;
    public Message Message { get; } = message;
    
    public bool IsOwnMessage() => Message.Author == Natsume;

    public bool IsNatsumeTagged() => Message.MentionedUsers.Contains(Natsume);
    public bool IsEveryoneTagged() => Message.MentionEveryone;
    public bool IsDirectMessage() => Message.Channel is DMChannel;
    
    public bool IsNatsumeInterested()
    {
        if (IsOwnMessage()) return false;
        if (IsNatsumeTagged()) return true;
        if (IsEveryoneTagged()) return true;
        if (IsDirectMessage()) return true;
        return false;
    }
}