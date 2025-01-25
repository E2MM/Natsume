using NetCord;
using NetCord.Gateway;

namespace Natsume.NetCord.NatsumeNetCordModules;

public class NatsumeListeningContext(Message message, User userNatsume)
{
    public string ContactName { get; set; } = message.Author.GetName();
    public User Natsume { get; set; } = userNatsume;
    public Message Message { get; set; } = message;
    
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