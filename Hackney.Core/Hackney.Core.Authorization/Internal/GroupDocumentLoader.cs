using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Hackney.Core.Authorization.Exceptions;
using Microsoft.Extensions.Hosting;

namespace Hackney.Core.Authorization.Internal
{
    // TODO: the SSM functionality will be needed in another nuget package, so it might make more sense
    // to have a dedicated ssm package with its own registration that we reference like we do with token factory.
    // This being an MVP, this will be left crude as is.
    internal sealed class GroupDocumentLoader : IHostedService
    {
        private static readonly TimeSpan FetchTimeout = TimeSpan.FromSeconds(5);

        private readonly PolicyGroupMap _groups;
        private readonly string _parameterName;
        private readonly IAmazonSimpleSystemsManagement? _ssmClient;

        public GroupDocumentLoader(
            PolicyGroupMap groups,
            string parameterName,
            IAmazonSimpleSystemsManagement? ssmClient)
        {
            _groups = groups;
            _parameterName = parameterName;
            _ssmClient = ssmClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(FetchTimeout);

            var ownsClient = _ssmClient is null;
            var client = _ssmClient ?? new AmazonSimpleSystemsManagementClient();
            try
            {
                var response = await client.GetParameterAsync(
                    new GetParameterRequest { Name = _parameterName, WithDecryption = true },
                    timeout.Token);

                _groups.Replace(ReadGroupMap(response.Parameter.Value));
            }
            catch (Exception ex) when (ex is not InvalidGroupDocumentException)
            {
                throw new InvalidOperationException(
                    $"Failed to fetch authorization groups from SSM parameter: '{_parameterName}'", ex);
            }
            finally
            {
                if (ownsClient)
                    client.Dispose();
            }
        }

        // one off task, not something that stays in the background, hence this StopAsync implementation is but a formality
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private PolicyGroupMap ReadGroupMap(string json)
        {
            try
            {
                return PolicyGroupMapMapper.Map(json);
            }
            catch (InvalidGroupDocumentException ex)
            {
                throw new InvalidGroupDocumentException(
                    $"Failed to read authorization groups from SSM parameter: '{_parameterName}'. {ex.Message}", ex);
            }
        }
    }
}
