using Microsoft.Extensions.Configuration;

namespace CampusHub.ConfigCenter.Configuration;

public static class TextConfigurationExtensions
{
    public static IConfigurationBuilder AddTextFile(this IConfigurationBuilder builder, string filePath)
    {
        return builder.Add(new TextConfigurationSource { FilePath = filePath });
    }
}
