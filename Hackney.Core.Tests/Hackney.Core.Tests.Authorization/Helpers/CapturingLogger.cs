using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;

namespace Hackney.Core.Tests.Authorization.Helpers;

internal sealed class CapturingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        Entries.Add((logLevel, formatter(state, exception)));

    public void ShouldHaveLogged(LogLevel level, string messagePart) =>
        Entries.Should().Contain(
            e => e.Level == level && e.Message.Contains(messagePart, StringComparison.OrdinalIgnoreCase),
            $"a {level} log containing '{messagePart}' was expected");
}
