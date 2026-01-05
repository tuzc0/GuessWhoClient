using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients
{
    public interface IUpdateProfileServiceClient : IDisposable
    {
        Task<WcfCallResult<bool>> UpdateUserProfileAsync(UpdateProfileRequest request);
        Task<WcfCallResult<bool>> DeleteUserProfileAsync(DeleteProfileRequest request);
        Task<WcfCallResult<GetProfileResponse>> GetProfileAsync(GetProfileRequest request);
    }
}