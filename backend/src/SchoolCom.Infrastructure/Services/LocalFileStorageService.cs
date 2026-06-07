using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Infrastructure.Services;

public class LocalFileStorageService(
    IHostEnvironment environment,
    IConfiguration configuration) : IFileStorage
{
    private string RootPath =>
        configuration["Storage:LocalPath"]
        ?? Path.Combine(environment.ContentRootPath, "uploads");

    public async Task<FileUploadResult> SaveAsync(
        Stream content,
        string fileName,
        string mimeType,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var safeName = Path.GetFileName(fileName);
        var storageKey = $"{tenantId:N}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(RootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);
        var size = fileStream.Length;

        return new FileUploadResult(storageKey, size);
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        if (storageKey.Contains("..", StringComparison.Ordinal))
            return Task.FromResult<Stream?>(null);

        var fullPath = Path.Combine(RootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public bool Exists(string storageKey)
    {
        if (storageKey.Contains("..", StringComparison.Ordinal)) return false;
        var fullPath = Path.Combine(RootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(fullPath);
    }
}
