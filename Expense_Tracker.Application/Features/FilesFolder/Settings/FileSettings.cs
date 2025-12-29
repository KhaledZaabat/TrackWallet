using System;
using System.Collections.Generic;
using System.Text;

namespace Expense_Tracker.Application.Features.FilesFolder.Settings;

public static class FileSettings
{
    public const int MaxFileSizeInMB = 10;
    public const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
    public const int MaxFileNameLength = 255;
    public const int MaxEntityTypeLength = 50;
    public const int FileHeaderSizeToRead = 8;

    public static readonly string[] AllowedImagesExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    ];

    public static readonly byte[][] BlockedSignatures =
    [
        new byte[] { 0x4D, 0x5A },                   // MZ (EXE/DLL)
        new byte[] { 0x3C, 0x3F, 0x70, 0x68 },       // <?ph (PHP)
        new byte[] { 0x3C, 0x21, 0x44, 0x4F },       // <!DO (HTML)
        new byte[] { 0x2F, 0x2F, 0x20 },             // //  (JS)
        new byte[] { 0x52, 0x61, 0x72, 0x21 },       // Rar
        new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C } // 7z
    ];

    public static readonly byte[][] AllowedSignatures =
    [
        new byte[] { 0x89, 0x50, 0x4E, 0x47 },       // PNG
        new byte[] { 0xFF, 0xD8, 0xFF },             // JPEG
        new byte[] { 0x47, 0x49, 0x46, 0x38 },       // GIF
        new byte[] { 0x52, 0x49, 0x46, 0x46 },       // WebP (RIFF)
        new byte[] { 0x25, 0x50, 0x44, 0x46 },       // PDF
        new byte[] { 0x50, 0x4B, 0x03, 0x04 },       // ZIP
        new byte[] { 0x49, 0x44, 0x33 },             // MP3 (ID3)
        new byte[] { 0xFF, 0xFB },                   // MP3 (frame)
        new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 }, // MP4 ftyp
        new byte[] { 0x66, 0x74, 0x79, 0x70 }        // MP4 (generic)
    ];
}
