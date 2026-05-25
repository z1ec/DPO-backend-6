namespace CampusHub.ConfigCenter.Models;

public class PortalOptions
{
    public string Title { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public AdminOptions Admin { get; set; } = new();
    public List<string> Modules { get; set; } = [];
}
