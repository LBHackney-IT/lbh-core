namespace Hackney.Core.JWT
{
    public class Token
    {
        public string[] Groups { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public int Nbf { get; set; }

        public int Exp { get; set; }

        public int Iat { get; set; }
    }
}