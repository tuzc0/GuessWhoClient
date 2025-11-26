using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;

namespace GuessWhoClient.Utilities
{
    public static class ServiceClientGuard
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServiceClientGuard));

        private const string LOG_ERROR_CLOSING_USER_SERVICE_CLIENT =
            "Error while closing service client. Aborting connection.";

        public static async Task CloseSafelyAsync(ICommunicationObject serviceClient)
        {
            if (serviceClient == null)
            {
                return;
            }

            try
            {
                if (serviceClient.State == CommunicationState.Faulted)
                {
                    serviceClient.Abort();
                    return;
                }

                await Task.Run(() => serviceClient.Close());
            }
            catch (TimeoutException ex)
            {
                Logger.Warn(LOG_ERROR_CLOSING_USER_SERVICE_CLIENT, ex);
                serviceClient.Abort();
            }
            catch (CommunicationException ex)
            {
                Logger.Warn(LOG_ERROR_CLOSING_USER_SERVICE_CLIENT, ex);
                serviceClient.Abort();
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Warn(LOG_ERROR_CLOSING_USER_SERVICE_CLIENT, ex);
                serviceClient.Abort();
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn(LOG_ERROR_CLOSING_USER_SERVICE_CLIENT, ex);
                serviceClient.Abort();
            }
            catch (Exception ex)
            {
                Logger.Warn(LOG_ERROR_CLOSING_USER_SERVICE_CLIENT, ex);
                serviceClient.Abort();
            }
        }
    }
}
