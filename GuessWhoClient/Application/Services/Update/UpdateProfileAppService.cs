using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Infraestructure.Wcf.Clients;
using GuessWhoClient.Interfaces;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services.Profile
{
    internal sealed class UpdateProfileAppService : IUpdateProfileAppService
    {
        private readonly IUpdateProfileServiceClient profileServiceClient;

        public UpdateProfileAppService(IUpdateProfileServiceClient profileServiceClient)
        {
            this.profileServiceClient = profileServiceClient ??
                throw new ArgumentNullException(nameof(profileServiceClient));
        }

        public Task<WcfCallResult<GetProfileResponse>> GetProfileAsync(GetProfileRequest request)
        {
            return profileServiceClient.GetProfileAsync(request);
        }

        public Task<WcfCallResult<bool>> UpdateProfileAsync(UpdateProfileRequest request)
        {
            return profileServiceClient.UpdateUserProfileAsync(request);
        }

        public Task<WcfCallResult<bool>> DeleteProfileAsync(DeleteProfileRequest request)
        {
            return profileServiceClient.DeleteUserProfileAsync(request);
        }
    }
}