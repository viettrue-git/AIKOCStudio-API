using Amazon.S3;
using Amazon.S3.Model;
using AiKocStudio.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AiKocStudio.Infrastructure.Storage;

/// <summary>
/// Server-proxied upload straight to MinIO (S3-compatible) — no presigned URLs,
/// per the Phase 3 plan's KISS decision. The bucket is created with a public-read
/// policy on first use so uploaded URLs are fetchable without signing.
/// </summary>
public class MinioFileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicEndpoint;

    public MinioFileStorageService(IConfiguration configuration)
    {
        var endpoint = configuration["Minio:Endpoint"]
            ?? throw new InvalidOperationException("Minio:Endpoint is not configured.");
        var accessKey = configuration["Minio:AccessKey"]
            ?? throw new InvalidOperationException("Minio:AccessKey is not configured.");
        var secretKey = configuration["Minio:SecretKey"]
            ?? throw new InvalidOperationException("Minio:SecretKey is not configured.");
        _bucketName = configuration["Minio:BucketName"]
            ?? throw new InvalidOperationException("Minio:BucketName is not configured.");
        _publicEndpoint = configuration["Minio:PublicEndpoint"]
            ?? throw new InvalidOperationException("Minio:PublicEndpoint is not configured.");

        _s3Client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
        });
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var key = $"{Guid.NewGuid()}-{fileName}";

        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
        }, cancellationToken);

        return $"{_publicEndpoint.TrimEnd('/')}/{_bucketName}/{key}";
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        if (await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName))
        {
            return;
        }

        try
        {
            await _s3Client.PutBucketAsync(_bucketName, cancellationToken);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists")
        {
            // Another concurrent first-upload created it between our check and this call — fine.
        }

        var publicReadPolicy = $$"""
            {
              "Version": "2012-10-17",
              "Statement": [
                {
                  "Effect": "Allow",
                  "Principal": "*",
                  "Action": "s3:GetObject",
                  "Resource": "arn:aws:s3:::{{_bucketName}}/*"
                }
              ]
            }
            """;

        await _s3Client.PutBucketPolicyAsync(_bucketName, publicReadPolicy, cancellationToken);
    }
}
