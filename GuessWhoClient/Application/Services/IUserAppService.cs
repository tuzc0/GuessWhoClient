using GuessWhoClient.Application.Results;
using GuessWhoClient.Domain.Models;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services
{
    public interface IUserAppService
    {
        Task<RegisterUserResult> RegisterAsync(UserRegistrationInput registrationInput);
    }
}
