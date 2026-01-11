using GuessWhoClient.Presentation.Infrastructure;
using GuessWhoClient.Infraestructure.ErrorHandling;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;

using Proxy = GuessWhoClient.FriendServiceRef;
using ContractRes = GuessWhoCore.Contracts.Response;
using ContractReq = GuessWhoCore.Contracts.Requests;

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
        private readonly IUiFaultMapper _faultMapper;

        public FriendServiceClientAdapter(IUiFaultMapper faultMapper)
        {
            _faultMapper = faultMapper ?? throw new ArgumentNullException(nameof(faultMapper));
        }

        public async Task<WcfCallResult<ContractRes.SearchProfilesResponse>> SearchProfilesAsync(ContractReq.SearchProfileRequest request)
        {
            Proxy.FriendServiceClient client = null;
            try
            {
                client = new Proxy.FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var proxyRequest = new Proxy.SearchProfileRequest { DisplayName = request.DisplayName };
                var response = await client.SearchProfilesAsync(proxyRequest);

                return WcfCallResult<ContractRes.SearchProfilesResponse>.Ok(new ContractRes.SearchProfilesResponse
                {
                    Profiles = response.Profiles?.Select(p => new ContractRes.UserProfileSearchResult
                    {
                        UserId = p.UserId,
                        DisplayName = p.DisplayName,
                        AvatarId = p.AvatarId
                    }).ToList()
                });
            }
            catch (Exception ex) { return HandleException<ContractRes.SearchProfilesResponse>(ex, nameof(SearchProfilesAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<ContractRes.SendFriendRequestResponse>> SendFriendRequestAsync(ContractReq.SendFriendRequestRequest request)
        {
            Proxy.FriendServiceClient client = null;
            try
            {
                client = new Proxy.FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var proxyRequest = new Proxy.SendFriendRequestRequest
                {
                    FromAccountId = request.FromAccountId,
                    ToUserId = request.ToUserId
                };
                var response = await client.SendFriendRequestAsync(proxyRequest);

                return WcfCallResult<ContractRes.SendFriendRequestResponse>.Ok(new ContractRes.SendFriendRequestResponse
                {
                    Success = response.Success,
                    AutoAccepted = response.AutoAccepted,
                    FriendRequestId = response.FriendRequestId
                });
            }
            catch (Exception ex) { return HandleException<ContractRes.SendFriendRequestResponse>(ex, nameof(SendFriendRequestAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<ContractRes.BasicResponse>> AcceptFriendRequestAsync(ContractReq.FriendRequestOperationRequest request)
        {
            Proxy.FriendServiceClient client = null;
            try
            {
                client = new Proxy.FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var proxyRequest = new Proxy.FriendRequestOperationRequest
                {
                    AccountId = request.AccountId,
                    FriendRequestId = request.FriendRequestId
                };
                var response = await client.AcceptFriendRequestAsync(proxyRequest);
                return WcfCallResult<ContractRes.BasicResponse>.Ok(new ContractRes.BasicResponse { Success = response.Success });
            }
            catch (Exception ex) { return HandleException<ContractRes.BasicResponse>(ex, nameof(AcceptFriendRequestAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<ContractRes.BasicResponse>> RejectFriendRequestAsync(ContractReq.FriendRequestOperationRequest request)
        {
            Proxy.FriendServiceClient client = null;
            try
            {
                client = new Proxy.FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var proxyRequest = new Proxy.FriendRequestOperationRequest
                {
                    AccountId = request.AccountId,
                    FriendRequestId = request.FriendRequestId
                };
                var response = await client.RejectFriendRequestAsync(proxyRequest);
                return WcfCallResult<ContractRes.BasicResponse>.Ok(new ContractRes.BasicResponse { Success = response.Success });
            }
            catch (Exception ex) { return HandleException<ContractRes.BasicResponse>(ex, nameof(RejectFriendRequestAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<ContractRes.BasicResponse>> CancelFriendRequestAsync(ContractReq.FriendRequestOperationRequest request)
        {
            Proxy.FriendServiceClient client = null;
            try
            {
                client = new Proxy.FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var proxyRequest = new Proxy.FriendRequestOperationRequest
                {
                    AccountId = request.AccountId,
                    FriendRequestId = request.FriendRequestId
                };
                var response = await client.CancelFriendRequestAsync(proxyRequest);
                return WcfCallResult<ContractRes.BasicResponse>.Ok(new ContractRes.BasicResponse { Success = response.Success });
            }
            catch (Exception ex) { return HandleException<ContractRes.BasicResponse>(ex, nameof(CancelFriendRequestAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<ContractRes.GetFriendsResponse>> GetFriendsAsync(ContractReq.GetFriendsRequest request)
        {
            Proxy.FriendServiceClient client = null;
            try
            {
                client = new Proxy.FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var proxyRequest = new Proxy.GetFriendsRequest { AccountId = request.AccountId };
                var response = await client.GetFriendsAsync(proxyRequest);

                return WcfCallResult<ContractRes.GetFriendsResponse>.Ok(new ContractRes.GetFriendsResponse
                {
                    Friends = response.Friends?.Select(f => new ContractRes.UserProfileSearchResult
                    {
                        UserId = f.UserId,
                        DisplayName = f.DisplayName,
                        AvatarId = f.AvatarId
                    }).ToList()
                });
            }
            catch (Exception ex) { return HandleException<ContractRes.GetFriendsResponse>(ex, nameof(GetFriendsAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<ContractRes.GetPendingRequestsResponse>> GetPendingRequestsAsync(ContractReq.GetPendingFriendRequestsRequest request)
        {
            Proxy.FriendServiceClient client = null;
            try
            {
                client = new Proxy.FriendServiceClient(WcfEndpointNames.FRIEND_SERVICE);
                var proxyRequest = new Proxy.GetPendingFriendRequestsRequest { AccountId = request.AccountId };
                var response = await client.GetPendingRequestsAsync(proxyRequest);

                return WcfCallResult<ContractRes.GetPendingRequestsResponse>.Ok(new ContractRes.GetPendingRequestsResponse
                {
                    Requests = response.Requests?.Select(r => new ContractRes.FriendRequest
                    {
                        FriendRequestId = r.FriendRequestId,
                        RequesterUserId = r.RequesterUserId,
                        RequesterDisplayName = r.RequesterDisplayName,
                        AddresseeUserId = r.AddresseeUserId,
                        StatusId = r.StatusId,
                        CreatedAt = r.CreatedAt
                    }).ToList()
                });
            }
            catch (Exception ex) { return HandleException<ContractRes.GetPendingRequestsResponse>(ex, nameof(GetPendingRequestsAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        private WcfCallResult<T> HandleException<T>(Exception ex, string context) where T : class
        {
            if (ex is FaultException<Proxy.ServiceFault> faultEx)
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