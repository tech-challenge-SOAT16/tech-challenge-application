namespace TechChallenge.Api.Configuration;

public class IntegracaoExternaOptions
{
    public const string SectionName = "IntegracaoExterna";
    public const string HeaderName = "X-Integration-Key";

    public string ApiKey { get; set; } = string.Empty;
}
