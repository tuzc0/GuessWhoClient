using GuessWhoClient.Application.Results;
using GuessWhoClient.Domain.Models;
using GuessWhoClient.Presentation.Infrastructure;
using GuessWhoClient.UserServiceRef;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services
{
    public class UserAppService : IUserAppService
    {
        private const string USER_SERVICE_ENDPOINT_NAME = "NetTcpBinding_IUserService";

        public async Task<RegisterUserResult> RegisterAsync(UserRegistrationInput registrationInput)
        {
            UserServiceClient client = null;

            try
            {
                client = new UserServiceClient(USER_SERVICE_ENDPOINT_NAME);

                var request = new RegisterRequest
                {
                    Email = registrationInput.Email,
                    DisplayName = registrationInput.Username,
                    Password = registrationInput.Password
                };

                RegisterResponse response = await client.RegisterUserAsync(request);

                return new RegisterUserResult(
                    response.EmailVerificationRequired,
                    response.AccountId,
                    response.Email);
            }
            finally
            {
                if(client != null)
                {
                    await ServiceClientGuard.CloseSafelyAsync(client);
                }
            }
        }
    }
}
