using GuessWhoClient.Presentation.Infrastructure;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf
{
    public sealed class WcfCallExecutor
    {
        public async Task<WcfCallResult<T>> CallAsync<TClient, T>(
            Func<TClient> clientFactory,
            Func<TClient, Task<T>> operationAsync,
            ILog logger,
            string logContext)
            where TClient : ICommunicationObject
        {
            if (clientFactory == null)
            {
                throw new ArgumentNullException(nameof(clientFactory));
            }

            if (operationAsync == null)
            {
                throw new ArgumentNullException(nameof(operationAsync));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            TClient client = default;

            try
            {
                WcfCallResult<TClient> createResult = await WcfCallErrorHelper.ExecuteAsync(
                    () => Task.FromResult(clientFactory.Invoke()),
                    logger,
                    logContext);

                if (!createResult.IsSuccess || !createResult.HasValue)
                {
                    return WcfCallResult<T>.Fail(createResult.FaultCode, createResult.ServerMessage);
                }

                client = createResult.Value;

                return await WcfCallErrorHelper.ExecuteAsync(
                    () => operationAsync(client),
                    logger,
                    logContext);
            }
            finally
            {
                if (client != null)
                {
                    await ServiceClientGuard.CloseSafelyAsync(client);
                }
            }
        }

        public async Task<WcfCallResult<bool>> CallVoidAsync<TClient>(
            Func<TClient> clientFactory,
            Func<TClient, Task> operationAsync,
            ILog logger,
            string logContext)
            where TClient : ICommunicationObject
        {
            if (operationAsync == null)
            {
                throw new ArgumentNullException(nameof (operationAsync));
            }

            return await CallAsync<TClient, bool>(
                clientFactory,
                async c =>
                {
                    await operationAsync(c);
                    return true;
                },
                logger,
                logContext);
        }
    }
}
