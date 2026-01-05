using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System.Threading.Tasks;

namespace GuessWhoClient.Interfaces
{
    public interface IUpdateProfileAppService
    {
        Task<WcfCallResult<bool>> UpdateProfileAsync(UpdateProfileRequest request);
        Task<WcfCallResult<bool>> DeleteProfileAsync(DeleteProfileRequest request);
        Task<WcfCallResult<GetProfileResponse>> GetProfileAsync(GetProfileRequest request);
    }
}