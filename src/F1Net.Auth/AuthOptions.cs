namespace F1Net.Auth;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public GoogleAuthOptions Google { get; set; } = new();
    public OpenIddictAppOptions Sync { get; set; } = new();
}

public class GoogleAuthOptions
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}

public class OpenIddictAppOptions
{
    public string ClientId { get; set; } = "f1net-sync";
    public string? ClientSecret { get; set; }
    public string DisplayName { get; set; } = "F1Net Weekly Sync";
}
