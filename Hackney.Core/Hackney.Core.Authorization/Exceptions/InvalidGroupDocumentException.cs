namespace Hackney.Core.Authorization.Exceptions;

public class InvalidGroupDocumentException : System.Exception
{
    public InvalidGroupDocumentException(string message) : base(message) { }

    public InvalidGroupDocumentException(string message, System.Exception innerException) : base(message, innerException) { }
}
