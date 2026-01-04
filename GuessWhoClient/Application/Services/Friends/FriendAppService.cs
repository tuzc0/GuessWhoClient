using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Infraestructure.Wcf.Clients;
using GuessWhoCore.Contracts.Request;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services.Friends
{
    internal sealed class FriendAppService : IFriendAppService
    {
        private readonly IFriendServiceClient friendServiceClient;

        public FriendAppService(IFriendServiceClient friendServiceClient)
        {
            this.friendServiceClient = friendServiceClient ??
                throw new ArgumentNullException(nameof(friendServiceClient));
        }

        public Task<WcfCallResult<SearchProfilesResponse>> SearchProfilesAsync(SearchProfileRequest request)
        {
            return friendServiceClient.SearchProfilesAsync(request);
        }

        public Task<WcfCallResult<SendFriendRequestResponse>> SendFriendRequestAsync(SendFriendRequestRequest request)
        {
            return friendServiceClient.SendFriendRequestAsync(request);
        }

        public Task<WcfCallResult<BasicResponse>> AcceptFriendRequestAsync(FriendRequestOperationRequest request)
        {
            return friendServiceClient.AcceptFriendRequestAsync(request);
        }

        public Task<WcfCallResult<BasicResponse>> RejectFriendRequestAsync(FriendRequestOperationRequest request)
        {
            return friendServiceClient.RejectFriendRequestAsync(request);
        }

        public Task<WcfCallResult<BasicResponse>> CancelFriendRequestAsync(FriendRequestOperationRequest request)
        {
            return friendServiceClient.CancelFriendRequestAsync(request);
        }

        public Task<WcfCallResult<GetFriendsResponse>> GetFriendsAsync(GetFriendsRequest request)
        {
            return friendServiceClient.GetFriendsAsync(request);
        }

        public Task<WcfCallResult<GetPendingRequestsResponse>> GetPendingRequestsAsync(GetPendingFriendRequestsRequest request)
        {
            return friendServiceClient.GetPendingRequestsAsync(request);
        }
    }
}