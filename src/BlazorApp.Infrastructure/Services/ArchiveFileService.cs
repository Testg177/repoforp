using System.Security.Cryptography;
using BlazorApp.Application.Interfaces;
using BlazorApp.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace BlazorApp.Infrastructure.Services;

public class ArchiveFileService : IArchiveFileService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public ArchiveFileService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    private string StoragePath => _config["Archive:DefaultStoragePath"] ?? "/app/archiwum";
    private long MaxFileSizeBytes => _config.GetValue<long>("Archive:MaxFileSizeMb", 50) * 1024 * 1024;

    private static readonly string[] DefaultAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".xlsx", ".docx"];

    public async Task<(string relativePath, string storedFileName, string checksum)> SaveFileAsync(
        Stream fileStream, string originalFileName, string category, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid()}{ext}";
        var subDir = Path.Combine(category, DateTime.UtcNow.ToString("yyyy/MM"));
        var fullDir = Path.Combine(StoragePath, subDir);
        Directory.CreateDirectory(fullDir);

        var fullPath = Path.Combine(fullDir, storedFileName);
        string checksum;

        await using (var fs = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs, ct);
        }

        await using (var fs = File.OpenRead(fullPath))
        {
            var hash = await SHA256.HashDataAsync(fs, ct);
            checksum = Convert.ToHexStringLower(hash);
        }

        var relativePath = Path.Combine(subDir, storedFileName);
        return (relativePath, storedFileName, checksum);
    }

    public async Task DeleteFileAsync(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        await Task.CompletedTask;
    }

    public string GetFullPath(string relativePath) =>
        Path.Combine(StoragePath, relativePath);

    public bool IsAllowedExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return DefaultAllowedExtensions.Contains(ext);
    }

    public bool IsWithinSizeLimit(long fileSizeBytes) =>
        fileSizeBytes <= MaxFileSizeBytes;
}
