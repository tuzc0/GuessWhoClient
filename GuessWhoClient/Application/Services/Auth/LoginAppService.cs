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

        public LoginAppService(ILoginServiceClient loginServiceClient)
        {
            this.loginServiceClient = loginServiceClient ?? 
                throw new ArgumentNullException(nameof(loginServiceClient));
        }

        public Task<WcfCallResult<LoginResponse>> LoginAsync(LoginRequest request)
        {
            return loginServiceClient.LoginUserAsync(request);
        }
    }
}
