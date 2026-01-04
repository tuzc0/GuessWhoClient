using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services.Auth
{
    public interface ILoginAppService
    {
        Task<WcfCallResult<LoginResponse>> LoginAsync(LoginRequest request);
    }
}
