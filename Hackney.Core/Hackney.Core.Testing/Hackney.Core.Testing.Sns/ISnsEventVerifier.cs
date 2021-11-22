using System;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.Sns
{
    public interface ISnsEventVerifier : IDisposable
    {
        Exception LastException { get; }

        Task<bool> VerifySnsEventRaised<T>(Action<T> verifyFunction) where T : class;
        Task PurgeQueueMessages();
    }
}
