using GuessWhoClient.FriendServiceRef;
using GuessWhoClient.Presentation.Infrastructure;
using GuessWhoCore.Contracts.Faults;
using GuessWhoCore.Contracts.Request;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients
{
    internal sealed class FriendServiceClientAdapter : IFriendServiceClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FriendServiceClientAdapter));

        private const string CODE_ENDPOINT_NOT_FOUND = "WCF_ENDPOINT_NOT_FOUND";
        private const string CODE_SECURITY = "WCF_SECURITY_ERROR";
        private const string CODE_TIMEOUT = "WCF_TIMEOUT";
        private const string CODE_COMMUNICATION = "WCF_COMMUNICATION_ERROR";
        private const string CODE_UNEXPECTED = "WCF_UNEXPECTED_ERROR";

        public async Task<WcfCallResult<SearchProfilesResponse>> SearchProfilesAsync(SearchProfileRequest request)
        {
            FriendServiceClient client = null;
            try
            {
                client = new FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var response = await client.SearchProfilesAsync(request);
                return WcfCallResult<SearchProfilesResponse>.Ok(response);
            }
            catch (Exception ex) { return HandleException<SearchProfilesResponse>(ex, nameof(SearchProfilesAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<SendFriendRequestResponse>> SendFriendRequestAsync(SendFriendRequestRequest request)
        {
            FriendServiceClient client = null;
            try
            {
                client = new FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var response = await client.SendFriendRequestAsync(request);
                return WcfCallResult<SendFriendRequestResponse>.Ok(response);
            }
            catch (Exception ex) { return HandleException<SendFriendRequestResponse>(ex, nameof(SendFriendRequestAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<BasicResponse>> AcceptFriendRequestAsync(FriendRequestOperationRequest request)
        {
            FriendServiceClient client = null;
            try
            {
                client = new FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var response = await client.AcceptFriendRequestAsync(request);
                return WcfCallResult<BasicResponse>.Ok(response);
            }
            catch (Exception ex) { return HandleException<BasicResponse>(ex, nameof(AcceptFriendRequestAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<BasicResponse>> RejectFriendRequestAsync(FriendRequestOperationRequest request)
        {
            FriendServiceClient client = null;
            try
            {
                client = new FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var response = await client.RejectFriendRequestAsync(request);
                return WcfCallResult<BasicResponse>.Ok(response);
            }
            catch (Exception ex) { return HandleException<BasicResponse>(ex, nameof(RejectFriendRequestAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<BasicResponse>> CancelFriendRequestAsync(FriendRequestOperationRequest request)
        {
            FriendServiceClient client = null;
            try
            {
                client = new FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var response = await client.CancelFriendRequestAsync(request);
                return WcfCallResult<BasicResponse>.Ok(response);
            }
            catch (Exception ex) { return HandleException<BasicResponse>(ex, nameof(CancelFriendRequestAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<GetFriendsResponse>> GetFriendsAsync(GetFriendsRequest request)
        {
            FriendServiceClient client = null;
            try
            {
                client = new FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var response = await client.GetFriendsAsync(request);
                return WcfCallResult<GetFriendsResponse>.Ok(response);
            }
            catch (Exception ex) { return HandleException<GetFriendsResponse>(ex, nameof(GetFriendsAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<GetPendingRequestsResponse>> GetPendingRequestsAsync(GetPendingFriendRequestsRequest request)
        {
            FriendServiceClient client = null;
            try
            {
                client = new FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var response = await client.GetPendingRequestsAsync(request);
                return WcfCallResult<GetPendingRequestsResponse>.Ok(response);
            }
            catch (Exception ex) { return HandleException<GetPendingRequestsResponse>(ex, nameof(GetPendingRequestsAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        private WcfCallResult<T> HandleException<T>(Exception ex, string context) where T : class
        {
            if (ex is FaultException<ServiceFault> faultEx)
            {
                Logger.Warn(context, faultEx);
                return WcfCallResult<T>.Fail(faultEx.Detail?.Code, faultEx.ToString());
            }

            Logger.Error(context, ex);

            if (ex is EndpointNotFoundException) return WcfCallResult<T>.Fail(CODE_ENDPOINT_NOT_FOUND, null);
            if (ex is MessageSecurityException) return WcfCallResult<T>.Fail(CODE_SECURITY, null);
            if (ex is TimeoutException) return WcfCallResult<T>.Fail(CODE_TIMEOUT, null);
            if (ex is CommunicationException) return WcfCallResult<T>.Fail(CODE_COMMUNICATION, null);

            return WcfCallResult<T>.Fail(CODE_UNEXPECTED, ex.Message);
        }
    }
}