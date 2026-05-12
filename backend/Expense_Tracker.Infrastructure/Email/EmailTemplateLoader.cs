using Expense_Tracker.Application.Interfaces;
using System.Reflection;

namespace Expense_Tracker.Infrastructure.Email;

public sealed class EmailTemplateLoader : IEmailTemplateLoader
{
    private readonly Assembly _assembly;

    public EmailTemplateLoader()
    {
        _assembly = typeof(EmailTemplateLoader).Assembly;
    }

    public async Task<string> LoadTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        var fileName = templateName.EndsWith(".html")
            ? templateName
            : $"{templateName}.html";

        var resourceName = $"{_assembly.GetName().Name.Replace("-", "_")}.Email.Templates.{fileName}";

        using var stream = _assembly.GetManifestResourceStream(resourceName);


        if (stream is null)
        {
            var availableResources = _assembly.GetManifestResourceNames();
            throw new FileNotFoundException(
                $"Email template not found: {resourceName}. " +
                $"Available: {string.Join(", ", availableResources)}");
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}