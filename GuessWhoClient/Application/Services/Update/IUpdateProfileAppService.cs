using GuessWhoClient.Infraestructure.Wcf; 
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System.Threading.Tasks;

namespace GuessWhoClient.Interfaces
{
    public interface IUpdateProfileAppService
    {
        Task<WcfCallResult<GetProfileResponse>> GetProfileAsync(GetProfileRequest request);
        Task<WcfCallResult<UpdateProfileResponse>> UpdateProfileAsync(UpdateProfileRequest request);
        Task<WcfCallResult<BasicResponse>> DeleteProfileAsync(DeleteProfileRequest request);
    }
}