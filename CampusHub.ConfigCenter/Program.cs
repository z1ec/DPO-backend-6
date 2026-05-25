using CampusHub.ConfigCenter.Configuration;
using CampusHub.ConfigCenter.Middleware;
using CampusHub.ConfigCenter.Models;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// --- Конфигурационные источники ---
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddXmlFile("portal.xml", optional: false, reloadOnChange: true)
    .AddIniFile("notifications.ini", optional: false, reloadOnChange: true)
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Portal:SupportEmail"] = "support-memory@campus.edu",
        ["Notifications:Sender"] = "notifications-memory@campus.edu",
        ["InMemory:Source"] = "AddInMemoryCollection"
    })
    .AddTextFile("customsettings.txt")
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// --- Регистрация IOptions ---
builder.Services.Configure<PortalOptions>(builder.Configuration.GetSection("Portal"));
builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("Notifications"));

var app = builder.Build();

app.UseMiddleware<PortalHeaderMiddleware>();

// --- Маршруты ---

app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head><meta charset="utf-8"><title>CampusHub.ConfigCenter</title></head>
    <body>
      <h1>CampusHub.ConfigCenter</h1>
      <p>Учебный сервис диагностики конфигурации ASP.NET Core.</p>
      <ul>
        <li><a href="/config/raw">/config/raw</a> — значения через IConfiguration</li>
        <li><a href="/config/section/portal">/config/section/portal</a> — секция Portal через GetSection()</li>
        <li><a href="/config/tree">/config/tree</a> — рекурсивная структура секции Portal</li>
        <li><a href="/config/connection">/config/connection</a> — строка подключения DefaultConnection</li>
        <li><a href="/config/providers">/config/providers</a> — список подключённых провайдеров</li>
        <li><a href="/config/custom">/config/custom</a> — данные из собственного провайдера (customsettings.txt)</li>
        <li><a href="/config/bind">/config/bind</a> — объект PortalOptions через Bind()</li>
        <li><a href="/config/options">/config/options</a> — IOptions&lt;PortalOptions&gt; и IOptions&lt;NotificationOptions&gt;</li>
        <li><a href="/config/effective">/config/effective</a> — итоговые значения конфликтующих ключей</li>
      </ul>
    </body>
    </html>
    """, "text/html"));

app.MapGet("/config/raw", (IConfiguration config) =>
{
    return Results.Ok(new
    {
        portal_title = config["Portal:Title"],
        portal_support_email = config["Portal:SupportEmail"],
        notifications_sender = config["Notifications:Sender"],
        feature_dark_mode = config["FeatureFlags:EnableDarkMode"],
        in_memory_source = config["InMemory:Source"]
    });
});

app.MapGet("/config/section/portal", (IConfiguration config) =>
{
    var section = config.GetSection("Portal");
    return Results.Ok(new
    {
        exists = section.Exists(),
        title = section["Title"],
        semester = section["Semester"],
        support_email = section["SupportEmail"],
        admin_name = section["Admin:Name"],
        admin_email = section["Admin:Email"],
        modules = section.GetSection("Modules").GetChildren().Select(c => c.Value).ToList()
    });
});

app.MapGet("/config/tree", (IConfiguration config) =>
{
    var section = config.GetSection("Portal");
    return Results.Ok(BuildTree(section));
});

app.MapGet("/config/connection", (IConfiguration config) =>
{
    return Results.Ok(new
    {
        default_connection = config.GetConnectionString("DefaultConnection")
    });
});

app.MapGet("/config/providers", (IConfiguration config) =>
{
    if (config is IConfigurationRoot root)
    {
        var providers = root.Providers.Select(p => p.GetType().Name).ToList();
        return Results.Ok(new { providers });
    }
    return Results.Ok(new { providers = Array.Empty<string>() });
});

app.MapGet("/config/custom", (IConfiguration config) =>
{
    var customSection = config.GetSection("Custom");
    var values = customSection.GetChildren()
        .ToDictionary(c => c.Key, c => c.Value);
    return Results.Ok(new { source = "customsettings.txt (TextConfigurationProvider)", values });
});

app.MapGet("/config/bind", (IConfiguration config) =>
{
    var portalOptions = config.GetSection("Portal").Get<PortalOptions>();
    return Results.Ok(portalOptions);
});

app.MapGet("/config/options", (
    Microsoft.Extensions.Options.IOptions<PortalOptions> portalOptions,
    Microsoft.Extensions.Options.IOptions<NotificationOptions> notificationOptions) =>
{
    return Results.Ok(new
    {
        portal = portalOptions.Value,
        notifications = notificationOptions.Value
    });
});

app.MapGet("/config/effective", (IConfiguration config) =>
{
    // Демонстрация конфликтов ключей:
    // Portal:Title  — appsettings.json < appsettings.Development.json < env vars < commandLineArgs
    // Portal:SupportEmail — appsettings.json < portal.xml < in-memory < env vars
    // Notifications:Sender — appsettings.json < notifications.ini < in-memory < env vars
    return Results.Ok(new
    {
        environment = app.Environment.EnvironmentName,
        conflicts = new
        {
            portal_title = new
            {
                value = config["Portal:Title"],
                sources = new[] { "appsettings.json", "appsettings.Development.json", "commandLineArgs" },
                winner = "commandLineArgs (последний источник побеждает)"
            },
            portal_support_email = new
            {
                value = config["Portal:SupportEmail"],
                sources = new[] { "appsettings.json", "portal.xml", "in-memory", "env vars" },
                winner = "env vars (Portal__SupportEmail) если задан, иначе in-memory"
            },
            notifications_sender = new
            {
                value = config["Notifications:Sender"],
                sources = new[] { "appsettings.json", "notifications.ini", "in-memory", "env vars" },
                winner = "env vars (Notifications__Sender) если задан, иначе in-memory"
            }
        }
    });
});

app.Run();

static Dictionary<string, object?> BuildTree(IConfigurationSection section)
{
    var children = section.GetChildren().ToList();
    if (!children.Any())
        return new Dictionary<string, object?> { [section.Key] = section.Value };

    var result = new Dictionary<string, object?>();
    foreach (var child in children)
    {
        var grandChildren = child.GetChildren().ToList();
        if (grandChildren.Any())
            result[child.Key] = BuildTree(child);
        else
            result[child.Key] = child.Value;
    }
    return result;
}
