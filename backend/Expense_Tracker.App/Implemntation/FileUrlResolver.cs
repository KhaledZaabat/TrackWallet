using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Expense_Tracker.App.Implemntation;

public sealed class FileUrlResolver(
    IHttpContextAccessor httpContextAccessor,
    IOptions<FileUrlOptions> options
) : IFileUrlResolver
{
    private const string FilesPath = "/api/files";

    public string? GetUrl(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            return null;

        string suffix = $"{FilesPath}/{id.Value}";

        string? configuredBase = options.Value.PublicBaseUrl;
        if (!string.IsNullOrWhiteSpace(configuredBase))
            return $"{configuredBase.TrimEnd('/')}{suffix}";

        HttpRequest? req = httpContextAccessor.HttpContext?.Request;
        if (req is not null && req.Host.HasValue)
            return $"{req.Scheme}://{req.Host.Value}{suffix}";

        return suffix;
    }
}
