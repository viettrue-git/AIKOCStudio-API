namespace AiKocStudio.Application.Common.Models;

/// <summary>
/// Checks the actual leading bytes of an upload against its declared content-type —
/// the allow-list in AllowedImageContentTypes only checks the client-supplied
/// header, which a malicious client can lie about.
/// </summary>
public static class ImageSignatureValidator
{
    public static bool MatchesContentType(Stream stream, string contentType)
    {
        if (!stream.CanSeek)
        {
            return false;
        }

        var originalPosition = stream.Position;
        stream.Position = 0;

        Span<byte> header = stackalloc byte[12];
        var bytesRead = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);

        stream.Position = originalPosition;

        return contentType switch
        {
            "image/jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => bytesRead >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            "image/webp" => bytesRead >= 12
                && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
                && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P',
            _ => false,
        };
    }
}
