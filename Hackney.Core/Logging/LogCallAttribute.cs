using AspectInjector.Broker;
using Microsoft.Extensions.Logging;
using System;

namespace Hackney.Core.Logging
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [Injection(typeof(LogCallAspect))]
    public class LogCallAttribute : Attribute
    {
        public LogLevel Level { get; set; } = LogLevel.Trace;
        public LogCallAttribute() { }
        public LogCallAttribute(LogLevel level)
        {
            Level = level;
        }
    }
}
