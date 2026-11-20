namespace Hackney.Core.Authorization.Exceptions;

internal class InvalidGroupDocumentException : System.Exception
{
    public InvalidGroupDocumentException(string message) : base(message) { }

    public InvalidGroupDocumentException(string message, System.Exception innerException) : base(message, innerException) { }
}
