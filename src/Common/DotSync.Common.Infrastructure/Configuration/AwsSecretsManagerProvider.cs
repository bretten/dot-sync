using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using com.brettnamba.DotSync.Common.Application.Configuration;

namespace com.brettnamba.DotSync.Common.Infrastructure.Configuration;

public sealed class AwsSecretsManagerProvider : ISecretsProvider
{
    private readonly string _secretName;
    private readonly AWSCredentials _credentials;
    private readonly RegionEndpoint _region;

    public AwsSecretsManagerProvider(string secretName, AWSCredentials credentials, RegionEndpoint region)
    {
        _secretName = secretName;
        _credentials = credentials;
        _region = region;
    }

    public async Task<Secrets> GetSecrets()
    {
        var request = new GetSecretValueRequest { SecretId = _secretName };
        using var client = new AmazonSecretsManagerClient(_credentials, new AmazonSecretsManagerConfig()
        {
            RegionEndpoint = _region
        });
        var response = await client.GetSecretValueAsync(request);
        return JsonSerializer.Deserialize<Secrets>(response.SecretString)!;
    }
}