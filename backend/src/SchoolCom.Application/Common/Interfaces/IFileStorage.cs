namespace SchoolCom.Application.Common.Interfaces;

public interface IFileStorage
{
    Task<FileUploadResult> SaveAsync(
        Stream content,
        string fileName,
        string mimeType,
        Guid tenantId,
        CancellationToken ct = default);

    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken ct = default);

    bool Exists(string storageKey);
}

public record FileUploadResult(string StorageKey, long SizeBytes);
