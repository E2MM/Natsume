using Microsoft.Extensions.Configuration;

namespace Natsume.Utils;

public static class ConfigurationManagerExtensions
{
    public static string GetValueOrThrow(
        this ConfigurationManager configuration,
        string section,
        string key)
    {
        var value = configuration.GetSection(section)[key];

        return value switch
        {
            null or "" => throw new ApplicationException($"Invalid Configuration, {section} {key} not found"),
            _ => value
        };
    }
}