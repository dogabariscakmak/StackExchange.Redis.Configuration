using Microsoft.Extensions.Configuration;

namespace StackExchange.Redis.Configuration;

public class RedisConfigurationSource : IConfigurationSource
{
    public readonly ConfigurationOptions ConfigurationOptions;
    public readonly string RedisConfigurationKey;
    public readonly bool ReloadOnChange;
    public readonly Action<ConfigurationRedisLoadExceptionContext>? OnLoadException;

    public RedisConfigurationSource(ConfigurationOptions configurationOptions,
        string redisConfigurationKey,
        bool reloadOnChange = true,
        Action<ConfigurationRedisLoadExceptionContext> onLoadException = null)
    {
        ConfigurationOptions = configurationOptions;
        RedisConfigurationKey = redisConfigurationKey;
        ReloadOnChange = reloadOnChange;
        OnLoadException = onLoadException;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new RedisConfigurationProvider(this);
}
