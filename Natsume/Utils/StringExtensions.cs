namespace Natsume.Utils;

public static class StringExtensions
{
    public static List<string> SplitForDiscord(this string text)
    {
        var splits = text.Split("//---DISCORD-SPLIT-MARKER---//");
        var result = new List<string>();
        
        foreach (var split in splits)
        {
            if (split.Length >= 2000)
            {
                var partSplits = split.Split('\n');
                var middle = partSplits.Length / 2;
                var firstPart = string.Join('\n', partSplits.Take(middle));
                var secondPart = string.Join('\n', partSplits.Skip(middle));
                result.Add(firstPart);
                result.Add(secondPart);
            }
            else if (string.IsNullOrWhiteSpace(split) is false)
            {
                result.Add(split);
            }
        }

        return result;
    }
}