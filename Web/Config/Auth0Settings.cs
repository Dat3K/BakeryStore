namespace Web.Config;

public class Auth0Settings
{
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ManagementApiClientId { get; set; } = string.Empty;
    public string ManagementApiClientSecret { get; set; } = string.Empty;
}
