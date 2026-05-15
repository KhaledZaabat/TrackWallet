using System.Security.Cryptography;

namespace Expense_Tracker.Infrastructure.Services;

/// <summary>
/// Stream wrapper that incrementally feeds every byte read from the inner
/// stream into a hash algorithm. Used by <see cref="FileService"/> to compute
/// a SHA-256 in lockstep with the upload, without ever holding the full file
/// in memory.
/// </summary>
internal sealed class HashingStream : Stream
{
    private readonly Stream _inner;
    private readonly IncrementalHash _hash;
    private bool _finalised;

    public HashingStream(Stream inner)
    {
        _inner = inner;
        _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = _inner.Read(buffer, offset, count);
        if (read > 0) _hash.AppendData(buffer, offset, read);
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
    {
        int read = await _inner.ReadAsync(buffer, ct);
        if (read > 0) _hash.AppendData(buffer.Span[..read]);
        return read;
    }

    public byte[] GetHashAndReset()
    {
        _finalised = true;
        return _hash.GetHashAndReset();
    }

    public override void Flush() => _inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_finalised) _hash.GetHashAndReset();
            _hash.Dispose();
            _inner.Dispose();
        }
        base.Dispose(disposing);
    }
}
