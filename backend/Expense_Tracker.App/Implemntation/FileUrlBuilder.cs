using Expense_Tracker.App.Controllers;
using Expense_Tracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Implemntation;

public sealed class FileUrlBuilder(HttpContext httpContext, IUrlHelper url) : IUrlBuilder
{


    public string? GetUrl(Guid? id)
    {
        if (!id.HasValue)
            return null;

        return url.Action(
            nameof(FilesController.GetFile),
            "Files",
            new { id = id.Value },
            httpContext.Request.Scheme
        );
    }
}
