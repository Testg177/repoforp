using System.Security.Cryptography;
using BlazorApp.Application.DTOs.Configuration;
using BlazorApp.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace BlazorApp.Infrastructure.Services;

public sealed class ArchiveFileService : IArchiveFileService
{
    private readonly ArchiveOptions _options;

    public ArchiveFileService(IOptions<ArchiveOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var targetDirectory = _options.DefaultStoragePath;
        Directory.CreateDirectory(targetDirectory);

        var fullPath = Path.Combine(targetDirectory, storedFileName);
        await using var fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return storedFileName;
    }

    public async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public async Task<bool> TestAccessAsync(string? storagePath = null, CancellationToken cancellationToken = default)
    {
        var path = storagePath ?? _options.DefaultStoragePath;
        try
        {
            Directory.CreateDirectory(path);
            var probeFile = Path.Combine(path, $".probe-{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(probeFile, "ok", cancellationToken);
            File.Delete(probeFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
