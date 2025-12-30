using Expense_Tracker.Application.Features.FilesFolder.Settings;

namespace Files.Contracts.Common;

///<summary>
/// Validates image file extension
/// </summary>
public class ImageExtensionValidator : FileExtensionValidator
{
    public ImageExtensionValidator() : base(FileSettings.AllowedImagesExtensions)
    {
    }
}