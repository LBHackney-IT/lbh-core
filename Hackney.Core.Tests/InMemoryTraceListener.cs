using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Hackney.Core.Tests
{
    public class InMemoryTraceListener : TraceListener
    {
        private readonly List<string> _traces = new List<string>();

        public void Reset()
        {
            _traces.Clear();
        }

        public bool ContainsTrace(string msg)
        {
            return _traces.Any(x => x.Contains(msg, StringComparison.CurrentCulture));
        }

        public override void Write(string message)
        {
            _traces.Add(message);
        }

        public override void WriteLine(string message)
        {
            _traces.Add(message);
        }
    }
}
