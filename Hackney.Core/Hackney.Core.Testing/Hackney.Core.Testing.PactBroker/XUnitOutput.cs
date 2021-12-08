using PactNet.Infrastructure.Outputters;
using Xunit.Abstractions;

namespace Hackney.Core.Testing.PactBroker
{
    /// <summary>
    /// Class to ensure pact broker verification output will get written to the xUnit test output stream
    /// </summary>
    public class XUnitOutput : IOutput
    {
        private readonly ITestOutputHelper _output;

        public XUnitOutput(ITestOutputHelper output)
        {
            _output = output;
        }

        public void WriteLine(string line)
        {
            _output.WriteLine(line);
        }
    }
}
