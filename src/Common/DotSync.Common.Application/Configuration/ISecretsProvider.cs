namespace com.brettnamba.DotSync.Common.Application.Configuration;

/// <summary>
/// TODO: Add an Amazon.SecretsManage implementation of a .NET Config provider
/// </summary>
public interface ISecretsProvider
{
    Task<Secrets> GetSecrets();
}

public sealed record Secrets(string ConnectionString, string SslCertPath, string SslCertPass);