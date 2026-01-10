using GuessWhoClient.LeaderboardServiceRef; // Asegúrate que este sea el nombre de tu referencia
using GuessWhoClient.Presentation.Infrastructure;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients
{
    internal sealed class LeaderboardServiceClientAdapter : ILeaderboardServiceClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LeaderboardServiceClientAdapter));

        private const string CODE_ENDPOINT_NOT_FOUND = "WCF_ENDPOINT_NOT_FOUND";
        private const string CODE_SECURITY = "WCF_SECURITY_ERROR";
        private const string CODE_TIMEOUT = "WCF_TIMEOUT";
        private const string CODE_COMMUNICATION = "WCF_COMMUNICATION_ERROR";
        private const string CODE_UNEXPECTED = "WCF_UNEXPECTED_ERROR";

        private readonly IUiFaultMapper _faultMapper;

        public LeaderboardServiceClientAdapter(IUiFaultMapper faultMapper)
        {
            _faultMapper = faultMapper ?? throw new ArgumentNullException(nameof(faultMapper));
        }

        public async Task<WcfCallResult<GetLeaderboardResponse>> GetGlobalLeaderboardAsync(GetLeaderboardRequest request)
        {
            LeaderboardServiceClient client = null;
            try
            {
                client = new LeaderboardServiceClient(WcfEndpointNames.LEADERBOARD_SERVICE);
                var response = await client.GetGlobalLeaderboardAsync(request);
                return WcfCallResult<GetLeaderboardResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException<GetLeaderboardResponse>(ex, nameof(GetGlobalLeaderboardAsync));
            }
            finally
            {
                await ServiceClientGuard.CloseSafelyAsync(client);
            }
        }

        private WcfCallResult<T> HandleException<T>(Exception ex, string context) where T : class
        {
            if (ex is FaultException<ServiceFault> faultEx)
            {
                Logger.Warn(context, faultEx);
                var mapping = _faultMapper.Map(faultEx.Detail?.Code);
                return WcfCallResult<T>.Fail(mapping.UiKey, faultEx.ToString());
            }

            Logger.Error(context, ex);

            if (ex is EndpointNotFoundException) return WcfCallResult<T>.Fail(CODE_ENDPOINT_NOT_FOUND, null);
            if (ex is MessageSecurityException) return WcfCallResult<T>.Fail(CODE_SECURITY, null);
            if (ex is TimeoutException) return WcfCallResult<T>.Fail(CODE_TIMEOUT, null);
            if (ex is CommunicationException) return WcfCallResult<T>.Fail(CODE_COMMUNICATION, null);

            return WcfCallResult<T>.Fail(CODE_UNEXPECTED, ex.Message);
        }

        public void Dispose() { }
    }
}