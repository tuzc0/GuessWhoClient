using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoCore.Contracts.Requests;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients
{
    public interface IUpdateProfileServiceClient : IDisposable
    {
        Task<WcfCallResult<bool>> UpdateUserProfileAsync(UpdateProfileRequest request);
        Task<WcfCallResult<bool>> DeleteUserProfileAsync(DeleteProfileRequest request);
    }
}