using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf
{
    public sealed class DuplexWcfCallExecutor
    {
        public Task<WcfCallResult<T>> CallAsync<TClient, T>(
            TClient client,
            Func<TClient, Task<T>> operationAsync,
            ILog logger,
            string logContext)
            where TClient : ICommunicationObject
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (operationAsync == null)
            {
                throw new ArgumentNullException(nameof(operationAsync));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            return WcfCallErrorHelper.ExecuteAsync(
                () => operationAsync(client),
                logger,
                logContext);
        }
    }
}
