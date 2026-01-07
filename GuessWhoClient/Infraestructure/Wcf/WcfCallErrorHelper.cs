using GuessWhoCore.Contracts.Faults;
using log4net;
using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf
{
    public sealed class WcfCallErrorHelper
    {
        private const string EMPTY = "";
        private const string ERROR_LOG_CONTEXT_REQUIRED = "logContext is required.";

        public static async Task<WcfCallResult<T>> ExecuteAsync<T>(
            Func<Task<T>> operationAsync,
            ILog logger,
            string logContext)
        {
            ValidateArguments(operationAsync, logger, logContext);

            try
            {
                T response = await operationAsync();
                return HandleSuccess(response, logger, logContext);
            }
            catch (FaultException<ServiceFault> ex)
            {
                return HandleServiceFault<T>(ex, logger, logContext);
            }
            catch (EndpointNotFoundException ex)
            {
                return HandleTechnicalFault<T>(ex, logger, logContext, WcfTechnicalFaultCodes.ENDPOINT_NOT_FOUND);
            }
            catch (SecurityNegotiationException ex)
            {
                return HandleTechnicalFault<T>(ex, logger, logContext, WcfTechnicalFaultCodes.SECURITY_ERROR);
            }
            catch (TimeoutException ex)
            {
                return HandleTechnicalFault<T>(ex, logger, logContext, WcfTechnicalFaultCodes.TIMEOUT);
            }
            catch (CommunicationException ex)
            {
                return HandleTechnicalFault<T>(ex, logger, logContext, WcfTechnicalFaultCodes.COMMUNICATION_ERROR);
            }
            catch (Exception ex)
            {
                return HandleTechnicalFault<T>(ex, logger, logContext, WcfTechnicalFaultCodes.UNEXPECTED);
            }
        }

        private static void ValidateArguments<T>(
            Func<Task<T>> operationAsync,
            ILog logger,
            string logContext)
        {
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
                throw new ArgumentException(ERROR_LOG_CONTEXT_REQUIRED, nameof(logContext));
            }
        }

        private static WcfCallResult<T> HandleSuccess<T>(T response, ILog logger, string logContext)
        {
            if (response is null)
            {
                logger.WarnFormat("{0}: null response.", logContext);
                return WcfCallResult<T>.Fail(WcfTechnicalFaultCodes.NULL_RESPONSE, EMPTY);
            }

            return WcfCallResult<T>.Ok(response);
        }


        private static WcfCallResult<T> HandleServiceFault<T>(
            FaultException<ServiceFault> ex,
            ILog logger,
            string logContext)
        {
            string code = ex.Detail != null ? ex.Detail.Code ?? EMPTY : EMPTY;
            string fallback = ex.Detail != null ? ex.Detail.FallbackMessage ?? EMPTY : EMPTY;

            logger.WarnFormat("{0}: Fault. Code='{1}', CorrelationId='{2}', MessageKey='{3}'.",
                logContext,
                code,
                ex.Detail != null ? ex.Detail.CorrelationId : EMPTY,
                ex.Detail != null ? ex.Detail.MessageKey : EMPTY);

            return WcfCallResult<T>.Fail(code, fallback);
        }

        private static WcfCallResult<T> HandleTechnicalFault<T>(
            Exception ex,
            ILog logger,
            string logContext,
            string technicalCode)
        {
            logger.Error(logContext, ex);
            return WcfCallResult<T>.Fail(technicalCode, EMPTY);
        }
    }
}
