using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services.Account
{
    public interface IUserAppService
    {
        Task<WcfCallResult<RegisterResponse>> RegisterUserAsync(RegisterRequest request);
        Task<WcfCallResult<VerifyEmailResponse>> VerifyEmailAsync(VerifyEmailRequest request);
        Task<WcfCallResult<bool>> ResendVerificationCodeAsync(ResendVerificationRequest request);
        Task<WcfCallResult<PasswordRecoveryResponse>> SendPasswordRecoveryCodeAsync(PasswordRecoveryRequest request);
        Task<WcfCallResult<bool>> UpdatePasswordAsync(UpdatePasswordRequest request);
    }
}
