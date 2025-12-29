using Expense_Tracker.Domain.Files;

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