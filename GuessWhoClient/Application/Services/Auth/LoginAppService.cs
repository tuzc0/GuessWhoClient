using GuessWhoClient.Infraestructure.Session;
using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Infraestructure.Wcf.Clients.Login;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services.Auth
{
    internal sealed class LoginAppService : ILoginAppService
    {
        private readonly ILoginServiceClient loginServiceClient;
        private readonly IPresenceHeartbeatService presenceHeartbeatService;

        public LoginAppService(
            ILoginServiceClient loginServiceClient,
            IPresenceHeartbeatService presenceHeartbeatService)
        {
            this.loginServiceClient = loginServiceClient ?? throw new ArgumentNullException(nameof(loginServiceClient));
            this.presenceHeartbeatService = presenceHeartbeatService ?? throw new ArgumentNullException(nameof(presenceHeartbeatService));
        }

        public async Task<WcfCallResult<LoginResponse>> LoginAsync(LoginRequest request)
        {
            WcfCallResult<LoginResponse> result = await loginServiceClient.LoginUserAsync(request);

            if (!result.IsSuccess || !result.HasValue || result.Value == null)
            {
                presenceHeartbeatService.Stop();
                return result;
            }

            if (!result.Value.ValidUser || result.Value.UserId <= 0)
            {
                presenceHeartbeatService.Stop();
                return result;
            }

            presenceHeartbeatService.Start(result.Value.UserId);
            return result;
        }
    }
}
