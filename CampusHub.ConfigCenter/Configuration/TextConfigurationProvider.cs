namespace CampusHub.ConfigCenter.Configuration;

public class TextConfigurationProvider : Microsoft.Extensions.Configuration.ConfigurationProvider
{
    private readonly string _filePath;

    public TextConfigurationProvider(string filePath)
    {
        _filePath = filePath;
    }

    public override void Load()
    {
        if (!File.Exists(_filePath))
            return;

        var lines = File.ReadAllLines(_filePath);
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i + 1 < lines.Length; i += 2)
        {
            var key = lines[i].Trim();
            var value = lines[i + 1].Trim();
            if (!string.IsNullOrEmpty(key))
                data[key] = value;
        }

        Data = data;
    }
}
