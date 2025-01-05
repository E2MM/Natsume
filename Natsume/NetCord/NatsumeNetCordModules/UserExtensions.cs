using NetCord;

namespace Natsume.NetCord.NatsumeNetCordModules;

public static class UserExtensions
{
    public static string GetName(this User user) => user.GlobalName ?? user.Username;
}