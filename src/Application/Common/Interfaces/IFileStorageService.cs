namespace AiKocStudio.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>Uploads a file and returns its publicly reachable URL.</summary>
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken);
}
