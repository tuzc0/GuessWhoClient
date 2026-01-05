using GuessWhoClient.Presentation.Infrastructure;
using GuessWhoCore.Contracts.Faults;
using log4net;
using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf
{
    public sealed class WcfCallExecutor
    {
        private const string EMPTY = "";

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

            if (string.IsNullOrWhiteSpace(logContext))
            {
                throw new ArgumentException("logContext is required.", nameof(logContext));
            }

            TClient client = default;

            try
            {
                client = clientFactory.Invoke();

                T response = await operationAsync(client);

                if (response == null)
                {
                    logger.WarnFormat("{0}: null response.", logContext);
                    return WcfCallResult<T>.Fail(WcfTechnicalFaultCodes.NULL_RESPONSE, EMPTY);
                }

                return WcfCallResult<T>.Ok(response);
            }
            catch (FaultException<ServiceFault> ex)
            {
                string code = ex.Detail != null ? ex.Detail.Code ?? EMPTY : EMPTY;
                string message = ex.Detail != null ? ex.Detail.MessageKey ?? EMPTY : EMPTY;

                logger.Warn(logContext, ex);
                return WcfCallResult<T>.Fail(code, message);
            }
            catch (EndpointNotFoundException ex)
            {
                logger.Error(logContext, ex);
                return WcfCallResult<T>.Fail(WcfTechnicalFaultCodes.ENDPOINT_NOT_FOUND, EMPTY);
            }
            catch (SecurityNegotiationException ex)
            {
                logger.Error(logContext, ex);
                return WcfCallResult<T>.Fail(WcfTechnicalFaultCodes.SECURITY_ERROR, EMPTY);
            }
            catch (TimeoutException ex)
            {
                logger.Error(logContext, ex);
                return WcfCallResult<T>.Fail(WcfTechnicalFaultCodes.TIMEOUT, EMPTY);
            }
            catch (CommunicationException ex)
            {
                logger.Error(logContext, ex);
                return WcfCallResult<T>.Fail(WcfTechnicalFaultCodes.COMMUNICATION_ERROR, EMPTY);
            }
            catch (Exception ex)
            {
                logger.Error(logContext, ex);
                return WcfCallResult<T>.Fail(WcfTechnicalFaultCodes.UNEXPECTED, EMPTY);
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
