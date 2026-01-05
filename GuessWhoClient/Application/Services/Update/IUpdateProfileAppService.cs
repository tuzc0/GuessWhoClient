using GuessWhoClient.Infraestructure.Wcf; 
using GuessWhoCore.Contracts.Requests;
using System.Threading.Tasks;

namespace GuessWhoClient.Interfaces
{
    public interface IUpdateProfileAppService
    {
        Task<WcfCallResult<bool>> UpdateProfileAsync(UpdateProfileRequest request);
        Task<WcfCallResult<bool>> DeleteProfileAsync(DeleteProfileRequest request);
    }
}