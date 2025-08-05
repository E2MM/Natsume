using Microsoft.Extensions.Configuration;

namespace Natsume.Utils;

public static class ConfigurationManagerExtensions
{
    public static string GetStringValueOrThrow(
        this ConfigurationManager configuration,
        string section,
        string key
    )
    {
        var value = configuration.GetSection(section)[key];

        return value switch
        {
            null => throw new ApplicationException(
                $"Invalid Configuration: expected valid string in section '{section}' for key '{key}' but found 'null'"),
            "" => throw new ApplicationException(
                $"Invalid Configuration: expected valid string in section '{section}' for key '{key}' but found empty string"),
            _ => value
        };
    }

    public static ulong GetUlongValueOrThrow(
        this ConfigurationManager configuration,
        string section,
        string key
    )
    {
        var value = configuration.GetSection(section)[key];
        var result = ulong.TryParse(value, out var guildId);

        return (result, value) switch
        {
            (_, null) => throw new ApplicationException(
                $"Invalid Configuration: expected valid string in section '{section}' for key '{key}' but found 'null'"),
            (_, "") => throw new ApplicationException(
                $"Invalid Configuration: expected valid string in section '{section}' for key '{key}' but found empty string"),
            (false, _) => throw new ApplicationException(
                $"Invalid Configuration: expected valid string in section '{section}' for key '{key}' but found '{value}'"),
            (true, _) => guildId
        };
    }
}