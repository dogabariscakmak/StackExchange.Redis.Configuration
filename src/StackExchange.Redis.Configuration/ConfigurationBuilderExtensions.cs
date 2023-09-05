using Microsoft.Extensions.Configuration;

namespace StackExchange.Redis.Configuration;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    ///     Adds the EFCore DbContext configuration provider with DbContextOptionsBuilder to builder.
    /// </summary>
    /// <param name="builder">The Microsoft.Extensions.Configuration.IConfigurationBuilder to add to.</param>
    /// <param name="configurationOptions">StackExchange.Redis ConfigurationOptions to connect Redis Server.</param>
    /// <param name="reloadOnChange">Auto reload when setting values changed.</param>
    /// <param name="application">Application name.</param>
    /// <param name="onLoadException">Error callback for exception when re/load settings from database.</param>
    /// <returns>The Microsoft.Extensions.Configuration.IConfigurationBuilder.</returns>
    public static IConfigurationBuilder AddRedisConfiguration(this IConfigurationBuilder builder,
        ConfigurationOptions configurationOptions,
        string redisConfigurationKey,
        bool reloadOnChange = true,
        Action<ConfigurationRedisLoadExceptionContext> onLoadException = null) =>
        builder.Add(new RedisConfigurationSource(configurationOptions, redisConfigurationKey, reloadOnChange, onLoadException));
}
