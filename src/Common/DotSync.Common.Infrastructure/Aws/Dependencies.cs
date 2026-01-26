using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using com.brettnamba.DotSync.Common.Application.Configuration;
using com.brettnamba.DotSync.Common.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace com.brettnamba.DotSync.Common.Infrastructure.Aws;

/// <summary>
/// AWS general dependencies
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Adds AWS dependencies
    /// </summary>
    public static void AddAws(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAmazonS3>(sp =>
        {
            if (!string.IsNullOrWhiteSpace(configuration["Aws:AccessKey"]))
            {
                var awsAccessKeyId = configuration["Aws:AccessKey"];
                var awsSecretAccessKey = configuration["Aws:SecretAccessKey"];
                var region = configuration["Aws:Region"];
                return new AmazonS3Client(new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey),
                    RegionEndpoint.GetBySystemName(region));
            }

            var chain = new CredentialProfileStoreChain();
            AWSConfigs.AWSProfileName = "roles_anywhere";
            if (!chain.TryGetAWSCredentials("roles_anywhere", out var credentials))
            {
                throw new Exception("Missing AWS credentials profile");
            }

            return new AmazonS3Client(credentials);
        });
    }

    public static ISecretsProvider GetSecretsProvider(this IConfiguration configuration)
    {
        var secretName = configuration["Aws:SecretsManager:SecretName"]!;
        var region = RegionEndpoint.GetBySystemName(configuration["Aws:SecretsManager:Region"]);
        if (!string.IsNullOrWhiteSpace(configuration["Aws:AccessKey"]))
        {
            var awsAccessKeyId = configuration["Aws:AccessKey"];
            var awsSecretAccessKey = configuration["Aws:SecretAccessKey"];

            return new AwsSecretsManagerProvider(secretName,
                new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey),
                region);
        }

        var chain = new CredentialProfileStoreChain();
        AWSConfigs.AWSProfileName = "roles_anywhere";
        if (!chain.TryGetAWSCredentials("roles_anywhere", out var credentials))
        {
            throw new Exception("Missing AWS credentials profile");
        }

        return new AwsSecretsManagerProvider(secretName, credentials, region);
    }
}