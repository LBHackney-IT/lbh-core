namespace Hackney.Core.JWT
{
    public class Token
    {
        public string Sub { get; set; }

        public string[] Groups { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public long Nbf { get; set; }

        public long Exp { get; set; }

        public long Iat { get; set; }
    }
}