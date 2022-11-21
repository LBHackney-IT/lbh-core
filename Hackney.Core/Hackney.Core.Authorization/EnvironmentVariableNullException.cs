namespace Hackney.Core.Authorization
{
    public class EnvironmentVariableNullException : System.Exception
    {
        public EnvironmentVariableNullException(string variableName) : base($"Cannot resolve {variableName} environment variable.") { }

    }
}