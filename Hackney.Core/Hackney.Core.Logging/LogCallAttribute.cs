using AspectInjector.Broker;
using Microsoft.Extensions.Logging;
using System;

namespace Hackney.Core.Logging
{
    /// <summary>
    /// Attribute to indicate that a method should have logging to indicate the start and end of its execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [Injection(typeof(LogCallAspect))]
    public class LogCallAttribute : Attribute
    {
        public LogLevel Level { get; set; } = LogLevel.Trace;
        public LogCallAttribute() { }

        /// <param name="level">The log level required</param>
        public LogCallAttribute(LogLevel level)
        {
            Level = level;
        }
    }
}
