using GuessWhoCore.Contracts.Request;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients
{
    public interface ILoginServiceClient
    {
        Task<WcfCallResult<LoginResponse>> LoginUserAsync(LoginRequest request);
        Task<WcfCallResult<BasicResponse>> LogoutUserAsync(LogoutRequest request);
    }
}
