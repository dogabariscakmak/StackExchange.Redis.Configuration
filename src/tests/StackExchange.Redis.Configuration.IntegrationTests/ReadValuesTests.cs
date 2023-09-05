using System.Diagnostics.Tracing;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using StackExchange.Redis.Configuration.IntegrationTests.Helpers;

using Xunit;

namespace StackExchange.Redis.Configuration.IntegrationTests;

[Collection("RedisServer")]
public class ReadValuesTests
{
    private RedisServerFixture _fixture;

    public ReadValuesTests(RedisServerFixture fixture)
    {
        _fixture = fixture;
        _fixture.ReloadTestData();
    }

    [Fact]
    public void ReadAndCheckValues()
    {
        //Arrange
        ConfigurationBuilder configuration = new ConfigurationBuilder();
        configuration.Sources.Clear();
        configuration.AddRedisConfiguration(ConfigurationOptions.Parse(_fixture.TestSettings.RedisConnectionConfiguration), "test");

        //Act
        IConfigurationRoot configurationRoot = configuration.Build();

        //Assert
        Assert.Equal(1, configurationRoot.GetValue<int>("Key1"));
        Assert.Equal(true, configurationRoot.GetValue<bool>("Key2"));
        Assert.Equal("Value3", configurationRoot.GetValue<string>("Key3"));
    }

    [Fact]
    public void ReadAndCheckValuesByIOptions()
    {
        //Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            return ConnectionMultiplexer.Connect(_fixture.TestSettings.RedisConnectionConfiguration);
        });

        ConfigurationBuilder configuration = new ConfigurationBuilder();
        configuration.Sources.Clear();
        configuration.AddRedisConfiguration(ConfigurationOptions.Parse(_fixture.TestSettings.RedisConnectionConfiguration), "test");

        //Act
        IConfigurationRoot configurationRoot = configuration.Build();

        services.AddOptions<MyConfigOptions>()
             .Bind(configurationRoot.GetSection(MyConfigOptions.MyConfig));
        var sp = services.BuildServiceProvider();

        //Assert
        var configs = sp.GetService<IOptions<MyConfigOptions>>();
        Assert.Equal(1, configs.Value.Key1);
        Assert.Equal(true, configs.Value.Key2);
        Assert.Equal("Value3", configs.Value.Key3);
    }

    [Fact]
    public async Task ReadAndCheckValuesByIOptionsMonitor()
    {
        //Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            return ConnectionMultiplexer.Connect(_fixture.TestSettings.RedisConnectionConfiguration);
        });

        ConfigurationBuilder configuration = new ConfigurationBuilder();
        configuration.Sources.Clear();
        configuration.AddRedisConfiguration(ConfigurationOptions.Parse(_fixture.TestSettings.RedisConnectionConfiguration), "test");

        //Act
        IConfigurationRoot configurationRoot = configuration.Build();

        services.AddOptions<MyConfigOptions>()
             .Bind(configurationRoot.GetSection(MyConfigOptions.MyConfig));
        var sp = services.BuildServiceProvider();

        //Assert
        var configs = sp.GetService<IOptionsMonitor<MyConfigOptions>>();
        Assert.Equal(1, configs.CurrentValue.Key1);
        Assert.Equal(true, configs.CurrentValue.Key2);
        Assert.Equal("Value3", configs.CurrentValue.Key3);

        var redis = sp.GetService<IConnectionMultiplexer>();
        redis.GetSubscriber().Publish("StackExchange_Redis_Configuration_Subscription:test",
            "{\r\n    \"MyConfig:Key1\": \"1\",\r\n    \"MyConfig:Key2\": \"true\",\r\n    \"MyConfig:Key3\": \"HasBeenChanged\"\r\n}");

        await Task.Delay(5 * 1000);

        Assert.Equal(1, configs.CurrentValue.Key1);
        Assert.Equal(true, configs.CurrentValue.Key2);
        Assert.Equal("HasBeenChanged", configs.CurrentValue.Key3);
    }
}
