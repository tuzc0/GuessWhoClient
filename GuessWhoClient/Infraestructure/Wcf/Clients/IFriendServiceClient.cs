using GuessWhoCore.Contracts.Request;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients
{
    public interface IFriendServiceClient
    {
        Task<WcfCallResult<SearchProfilesResponse>> SearchProfilesAsync(SearchProfileRequest request);
        Task<WcfCallResult<SendFriendRequestResponse>> SendFriendRequestAsync(SendFriendRequestRequest request);
        Task<WcfCallResult<BasicResponse>> AcceptFriendRequestAsync(FriendRequestOperationRequest request);
        Task<WcfCallResult<BasicResponse>> RejectFriendRequestAsync(FriendRequestOperationRequest request);
        Task<WcfCallResult<BasicResponse>> CancelFriendRequestAsync(FriendRequestOperationRequest request);
        Task<WcfCallResult<GetFriendsResponse>> GetFriendsAsync(GetFriendsRequest request);
        Task<WcfCallResult<GetPendingRequestsResponse>> GetPendingRequestsAsync(GetPendingFriendRequestsRequest request);
    }
}