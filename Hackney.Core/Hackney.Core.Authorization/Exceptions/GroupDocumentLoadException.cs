using System;

namespace Hackney.Core.Authorization.Exceptions;

/// <summary>
/// The group document could not be loaded when the host started.
/// </summary>
public sealed class GroupDocumentLoadException : Exception
{
    /// <summary>
    /// The Parameter Store parameter the loader asked for.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// <paramref name="innerException"/> for why the load failed.
    /// </summary>
    public GroupDocumentLoadException(string parameterName, Exception innerException)
        : base($"Failed to load authorization groups from SSM parameter: '{parameterName}'.", innerException)
    {
        ParameterName = parameterName;
    }
}
