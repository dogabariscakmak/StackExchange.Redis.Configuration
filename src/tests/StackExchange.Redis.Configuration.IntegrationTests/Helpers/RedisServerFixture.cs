using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace StackExchange.Redis.Configuration.IntegrationTests.Helpers;

public class RedisServerFixture : IDisposable
{
    public ServiceProvider DI { get; private set; }
    public TestSettings TestSettings { get; private set; }
    public Dictionary<string, object> TestData { get; private set; }

    private readonly DockerStarter _dockerStarter;
    private bool _disposed;

    public RedisServerFixture()
    {
        TestData = new Dictionary<string, object>();

        var config = new ConfigurationBuilder().AddJsonFile("testsettings.json").Build();

        bool IsGithubAction = false;
        bool.TryParse(Environment.GetEnvironmentVariable("IS_GITHUB_ACTION"), out IsGithubAction);

        TestSettings = new TestSettings()
        {
            RedisConnectionConfiguration = config.GetSection(TestSettings.Position)["RedisConnectionConfiguration"],
            DockerComposeFile = config.GetSection(TestSettings.Position)["DockerComposeFile"],
            DockerWorkingDir = config.GetSection(TestSettings.Position)["DockerWorkingDir"],
            DockerComposeExePath = config.GetSection(TestSettings.Position)["DockerComposeExePath"],
            TestDataFilePath = config.GetSection(TestSettings.Position)["TestDataFilePath"],
            IsGithubAction = IsGithubAction
        };

        if (!TestSettings.IsGithubAction)
        {
            _dockerStarter = new DockerStarter(TestSettings.DockerComposeExePath, TestSettings.DockerComposeFile, TestSettings.DockerWorkingDir);
            _dockerStarter.Start();
        }

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            return ConnectionMultiplexer.Connect(TestSettings.RedisConnectionConfiguration);
        });

        DI = services.BuildServiceProvider();
    }

    public void ReloadTestData()
    {
        TestData.Clear();
        using (StreamReader file = File.OpenText(TestSettings.TestDataFilePath))
        {
            TestData = (Dictionary<string, object>)JsonSerializer.Deserialize(file.ReadToEnd(), typeof(Dictionary<string, object>));
        }

        var connectionMultiplexer = (ConnectionMultiplexer)DI.GetService<IConnectionMultiplexer>();
        var server = connectionMultiplexer.GetServer("localhost:6379");
        server.FlushDatabase();

        connectionMultiplexer.GetDatabase().HashSet("StackExchange_Redis_Configuration:test", TestData.Select(pair => new HashEntry(pair.Key, pair.Value.ToString())).ToArray());

    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DI.Dispose();
                if (!TestSettings.IsGithubAction)
                {
                    _dockerStarter.Dispose();
                }
            }

            _disposed = true;
        }
    }
}

[CollectionDefinition("RedisServer")]
public class RedisCollection : ICollectionFixture<RedisServerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
