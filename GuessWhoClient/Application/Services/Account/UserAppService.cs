using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.UserServiceRef;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services.Account
{
    public class UserAppService : IUserAppService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UserAppService));

        private const string LOG_CTX_REGISTER = "UserAppService.RegisterUser";
        private const string LOG_CTX_VERIFY = "UserAppService.VerifyEmail";
        private const string LOG_CTX_RESEND = "UserAppService.ResendVerification";
        private const string LOG_CTX_SEND_RECOVERY = "UserAppService.SendRecoveryCode";
        private const string LOG_CTX_UPDATE_PASSWORD = "UserAppService.UpdatePassword";

        private readonly WcfCallExecutor wcfCallExecutor;

        public UserAppService(WcfCallExecutor wcfCallExecutor)
        {
            this.wcfCallExecutor = wcfCallExecutor ?? 
                throw new ArgumentNullException(nameof(wcfCallExecutor));
        }

        public Task<WcfCallResult<RegisterResponse>> RegisterUserAsync(RegisterRequest request)
        {
            return wcfCallExecutor.CallAsync<UserServiceClient, RegisterResponse>(
                () => new UserServiceClient(WcfEndpointNames.USER_SERVICE),
                c => c.RegisterUserAsync(request),
                Logger,
                LOG_CTX_REGISTER);
        }

        public Task<WcfCallResult<VerifyEmailResponse>> VerifyEmailAsync(VerifyEmailRequest request)
        {
            return wcfCallExecutor.CallAsync<UserServiceClient, VerifyEmailResponse>(
                () => new UserServiceClient(WcfEndpointNames.USER_SERVICE),
                c => c.ConfirmEmailAddressWithVerificationCodeAsync(request),
                Logger,
                LOG_CTX_VERIFY);
        }

        public Task<WcfCallResult<bool>> ResendVerificationCodeAsync(ResendVerificationRequest request)
        {
            return wcfCallExecutor.CallVoidAsync<UserServiceClient>(
                () => new UserServiceClient(WcfEndpointNames.USER_SERVICE),
                c => c.ResendEmailVerificationCodeAsync(request),
                Logger, 
                LOG_CTX_RESEND);
        }

        public Task<WcfCallResult<PasswordRecoveryResponse>> SendPasswordRecoveryCodeAsync(PasswordRecoveryRequest request)
        {
            return wcfCallExecutor.CallAsync<UserServiceClient, PasswordRecoveryResponse>(
                () => new UserServiceClient(WcfEndpointNames.USER_SERVICE),
                c => c.SendPasswordRecoveryCodeAsync(request),
                Logger,
                LOG_CTX_SEND_RECOVERY);
        }

        public Task<WcfCallResult<bool>> UpdatePasswordAsync(UpdatePasswordRequest request)
        {
            return wcfCallExecutor.CallAsync<UserServiceClient, bool>(
                () => new UserServiceClient(WcfEndpointNames.USER_SERVICE),
                c => c.UpdatePasswordWithVerificationCodeAsync(request),
                Logger,
                LOG_CTX_UPDATE_PASSWORD);
        }
    }
}
