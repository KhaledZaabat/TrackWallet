using Expense_Tracker.Application.Features.FilesFolder.Settings;
using Microsoft.AspNetCore.Http;

public static class FileHelper
{
    /// <summary>
    /// Reads the file header (first N bytes) from an uploaded file
    /// </summary>
    public static async Task<byte[]> GetFileHeaderAsync(
        IFormFile file,
        CancellationToken ct = default,
        int headerSize = FileSettings.FileHeaderSizeToRead)
    {
        await using var stream = file.OpenReadStream();
        var header = new byte[headerSize];

        try
        {
            await stream.ReadExactlyAsync(header, ct);
        }
        catch (EndOfStreamException)
        {
            var bytesRead = (int)stream.Position;
            Array.Resize(ref header, bytesRead);
        }

        return header;
    }

    /// <summary>
    /// Checks if file bytes start with a specific signature
    /// </summary>
    public static bool StartsWith(byte[] fileBytes, byte[] signature)
    {
        if (fileBytes.Length < signature.Length)
            return false;

        for (int i = 0; i < signature.Length; i++)
        {
            if (fileBytes[i] != signature[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the file signature is in the blocked list
    /// </summary>
    public static bool IsSignatureBlocked(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return false;

        foreach (var signature in FileSettings.BlockedSignatures)
        {
            if (StartsWith(fileBytes, signature))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the file signature is in the allowed list
    /// </summary>
    public static bool IsSignatureAllowed(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return false;

        foreach (var signature in FileSettings.AllowedSignatures)
        {
            if (StartsWith(fileBytes, signature))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Validates file signature against allowed and blocked lists
    /// </summary>
    public static async Task<bool> ValidateFileSignatureAsync(
        IFormFile file,
        CancellationToken ct = default)
    {
        var header = await GetFileHeaderAsync(file, ct);

        // First check if blocked
        if (IsSignatureBlocked(header))
            return false;

        // Then check if allowed
        return IsSignatureAllowed(header);
    }

    /// <summary>
    /// Validates file extension against allowed list
    /// </summary>
    public static bool IsExtensionAllowed(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return FileSettings.AllowedImagesExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates file size
    /// </summary>
    public static bool IsFileSizeValid(long fileSizeInBytes)
    {
        return fileSizeInBytes > 0 && fileSizeInBytes <= FileSettings.MaxFileSizeInBytes;
    }
}