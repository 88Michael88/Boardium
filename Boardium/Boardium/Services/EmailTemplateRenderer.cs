namespace Boardium.Services;

public class EmailTemplateRenderer
{
    private readonly string _templatePath;
    private readonly ILogger<EmailTemplateRenderer> _logger;

    public EmailTemplateRenderer(ILogger<EmailTemplateRenderer> logger,IWebHostEnvironment env)
    {
        _logger = logger;
        _templatePath = Path.Combine(env.ContentRootPath, "EmailTemplates");
    }

    public string Render(string templateName, Dictionary<string, string> placeholders)
    {
        var fullPath = Path.Combine(_templatePath, $"{templateName}.html");

        if (!File.Exists(fullPath))
        {
            _logger.LogError("Email template '{TemplateName}' not found at path: {Path}", templateName, fullPath);
            return $"<p><b>Template '{templateName}' not found.</b></p>";
        }

        var content = File.ReadAllText(fullPath);

        foreach (var (key, value) in placeholders)
        {
            content = content.Replace($"{{{{{key}}}}}", value);
        }

        return content;
    }
}