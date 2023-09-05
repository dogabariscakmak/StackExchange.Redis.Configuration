using System.Text.Json;

using Microsoft.Extensions.Configuration;

namespace StackExchange.Redis.Configuration;

public class RedisConfigurationProvider : ConfigurationProvider, IDisposable
{
    private static readonly string RedisConfigurationKeyPrefix = "StackExchange_Redis_Configuration";
    private static readonly string RedisSubscriptionChannelPrefix = "StackExchange_Redis_Configuration_Subscription";

    private readonly RedisConfigurationSource _configurationSource;
    private bool _disposed;
    private readonly string _redisConfigurationKey;
    private readonly string _redisSubscriptionKey;
    private bool _isSubscribed = false;

    public RedisConfigurationProvider(RedisConfigurationSource configurationSource)
    {
        _configurationSource = configurationSource;

        _redisConfigurationKey = $"{RedisConfigurationKeyPrefix}:{_configurationSource.RedisConfigurationKey}";
        _redisSubscriptionKey = $"{RedisSubscriptionChannelPrefix}:{_configurationSource.RedisConfigurationKey}";
    }

    public override void Load()
    {
        try
        {
            Data = GetData();
        }
        catch (Exception ex)
        {
            var exceptionContext = new ConfigurationRedisLoadExceptionContext(_configurationSource, ex);
            _configurationSource.OnLoadException?.Invoke(exceptionContext);
            if (!exceptionContext.Ignore)
            {
                throw;
            }
        }

        if (_configurationSource.ReloadOnChange && !_isSubscribed)
        {
            SubscribeRedis();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        RedisConnection.GetInstance(_configurationSource.ConfigurationOptions).Connection?.Dispose();
        _disposed = true;
    }

    private IDictionary<string, string>? GetData() =>
        RedisConnection.GetInstance(_configurationSource.ConfigurationOptions)
        .Connection?.GetDatabase().HashGetAll(_redisConfigurationKey).ToStringDictionary();

    private void SubscribeRedis()
    {
        try
        {
            ISubscriber? subscriber = RedisConnection.GetInstance(_configurationSource.ConfigurationOptions).Connection?.GetSubscriber();
            
            if(subscriber is null)
            {
                return;
            }

            subscriber.Subscribe(_redisSubscriptionKey, (channel, message) =>
            {
                Dictionary<string, string?>? newData = JsonSerializer.Deserialize<Dictionary<string, string?>>(message.ToString());
                if(newData is null)
                {
                    return;
                }

                Data = newData;
                OnReload();
            });

            _isSubscribed = true;

        }
        catch (Exception ex)
        {
            var exceptionContext = new ConfigurationRedisLoadExceptionContext(_configurationSource, ex);
            _configurationSource.OnLoadException?.Invoke(exceptionContext);
            if (!exceptionContext.Ignore)
            {
                throw;
            }
        }
    }
}
