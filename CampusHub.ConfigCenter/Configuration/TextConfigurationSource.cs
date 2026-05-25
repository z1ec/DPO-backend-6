using Microsoft.Extensions.Configuration;

namespace CampusHub.ConfigCenter.Configuration;

public class TextConfigurationSource : IConfigurationSource
{
    public string FilePath { get; set; } = string.Empty;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new TextConfigurationProvider(FilePath);
    }
}
