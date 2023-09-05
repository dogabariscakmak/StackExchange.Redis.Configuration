namespace StackExchange.Redis.Configuration.IntegrationTests.Helpers;
public class TestSettings
{
    public const string Position = "TestSettings";
    public string RedisConnectionConfiguration { get; set; }
    public string DockerComposeFile { get; set; }
    public string DockerWorkingDir { get; set; }
    public string DockerComposeExePath { get; set; }
    public string TestDataFilePath { get; set; }
    public bool IsGithubAction { get; set; }
}
