using GuessWhoCore.Contracts.Request;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System.Threading.Tasks;

namespace GuessWhoClient.Interfaces
{
    public interface IFriendManager
    {
        Task<SearchProfilesResponse> SearchProfilesAsync(SearchProfileRequest request);

        Task<SendFriendRequestResponse> SendRequestAsync(SendFriendRequestRequest request);

        Task<BasicResponse> AcceptFriendRequestAsync(FriendRequestOperationRequest request);

        Task<BasicResponse> RejectFriendRequestAsync(FriendRequestOperationRequest request);

        Task<BasicResponse> CancelFriendRequestAsync(FriendRequestOperationRequest request);

        Task<GetFriendsResponse> GetFriendsAsync(GetFriendsRequest request);

        Task<GetPendingRequestsResponse> GetPendingRequestsAsync(GetPendingFriendRequestsRequest request);
    }
}