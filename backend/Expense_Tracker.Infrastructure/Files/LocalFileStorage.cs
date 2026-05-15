using Expense_Tracker.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Expense_Tracker.Infrastructure.Files;


public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(IWebHostEnvironment env)
    {
        _rootPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "AppData"));
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<long> SaveAsync(string key, Stream content, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        string absolute = ResolveSafe(key);

        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

      
        string tmp = absolute + ".tmp-" + Guid.NewGuid().ToString("N");
        long bytesWritten;

        try
        {
            await using (var dest = new FileStream(
                tmp,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                useAsync: true))
            {
                await content.CopyToAsync(dest, ct);
                bytesWritten = dest.Length;
            }

            File.Move(tmp, absolute, overwrite: true);
            return bytesWritten;
        }
        catch
        {
           
            TryDelete(tmp);
            throw;
        }
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken ct = default)
    {
        string absolute = ResolveSafe(key);
        if (!File.Exists(absolute))
            return Task.FromResult<Stream?>(null);

        Stream s = new FileStream(
            absolute,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            useAsync: true);

        return Task.FromResult<Stream?>(s);
    }

    public Task<bool> DeleteAsync(string key, CancellationToken ct = default)
    {
        string absolute = ResolveSafe(key);
        if (!File.Exists(absolute))
            return Task.FromResult(false);

        File.Delete(absolute);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        string absolute = ResolveSafe(key);
        return Task.FromResult(File.Exists(absolute));
    }


    private string ResolveSafe(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key is required.", nameof(key));

        string normalized = key.Replace('\\', '/').TrimStart('/');
        string absolute = Path.GetFullPath(Path.Combine(_rootPath, normalized));

        if (!absolute.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Key resolves outside storage root.");

        return absolute;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch {  }
    }
}
