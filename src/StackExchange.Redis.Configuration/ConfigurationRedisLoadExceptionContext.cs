namespace StackExchange.Redis.Configuration;

public class ConfigurationRedisLoadExceptionContext
{
    public Exception Exception { get; }
    public bool Ignore { get; set; }
    public RedisConfigurationSource Source { get; }

    internal ConfigurationRedisLoadExceptionContext(RedisConfigurationSource source, Exception exception)
    {
        Source = source;
        Exception = exception;
    }
}
