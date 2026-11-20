using System;
using System.Collections.Generic;
using System.IO;

namespace Hackney.Core.Tests.Authorization.Helpers;

/// <summary>
/// Hide the developer machine's AWS profile, region, and metadata service so the
/// parameter-store client constructed by the package fails before it opens a connection.
/// The variables are process-wide and restored on dispose. This is safe while no other
/// test in this assembly constructs a real AWS client. A new test that does must not run
/// alongside this one.
/// </summary>
internal sealed class ClearedAwsDiscovery : IDisposable
{
    private readonly Dictionary<string, string?> _previous = [];
    private readonly string _configPath = Path.GetTempFileName();
    private readonly string _credentialsPath = Path.GetTempFileName();

    public ClearedAwsDiscovery()
    {
        Set("AWS_REGION", null);
        Set("AWS_DEFAULT_REGION", null);
        Set("AWS_PROFILE", null);
        Set("AWS_ACCESS_KEY_ID", "test-key");
        Set("AWS_SECRET_ACCESS_KEY", "test-secret");
        Set("AWS_SESSION_TOKEN", null);
        Set("AWS_EC2_METADATA_DISABLED", "true");
        Set("AWS_CONTAINER_CREDENTIALS_RELATIVE_URI", null);
        Set("AWS_CONTAINER_CREDENTIALS_FULL_URI", null);
        Set("AWS_SHARED_CREDENTIALS_FILE", _credentialsPath);
        Set("AWS_CONFIG_FILE", _configPath);
    }

    public void Dispose()
    {
        foreach (var (name, value) in _previous)
            Environment.SetEnvironmentVariable(name, value);

        File.Delete(_configPath);
        File.Delete(_credentialsPath);
    }

    private void Set(string name, string? value)
    {
        _previous[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
    }
}
