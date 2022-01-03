using System;

namespace Hackney.Core.Authorization.Exceptions
{
    public class EnvironmentVariableIsNullException : Exception
    {
        public EnvironmentVariableIsNullException(string message):
            base(message)
        { }


    }
}
