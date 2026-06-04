namespace Hackney.Core.JWT;

public class Token
{
    public required string Sub { get; set; }
    public required string[] Groups { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public long Nbf { get; set; }
    public long Exp { get; set; }
    public long Iat { get; set; }
}
