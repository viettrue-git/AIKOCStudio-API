namespace AiKocStudio.Application.Common.Models;

public static class AllowedImageContentTypes
{
    public static readonly string[] Values = ["image/jpeg", "image/png", "image/webp"];

    public const long MaxSizeBytes = 5 * 1024 * 1024;
}
