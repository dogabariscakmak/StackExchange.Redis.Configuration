namespace StackExchange.Redis.Configuration;

internal class RedisConnection
{
    private Lazy<ConnectionMultiplexer>? LazyConnection;
    public ConnectionMultiplexer? Connection
    {
        get
        {
            return LazyConnection?.Value;
        }
    }

    private static RedisConnection? Instance = null;
    private static readonly object Padlock = new object();

    private RedisConnection(ConfigurationOptions configurationOptions)
    {
        LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(configurationOptions);
        });
    }

    public static RedisConnection GetInstance(ConfigurationOptions configurationOptions)
    {
        lock (Padlock)
        {
            if (Instance == null)
            {
                Instance = new RedisConnection(configurationOptions);
            }
            return Instance;
        }
    }
}

