using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Interfaces;
using GuessWhoClient.UpdateServiceRef;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Application.Services.Profile
{
    public sealed class UpdateProfileAppService : IUpdateProfileAppService
    {
        private readonly ILog Logger = LogManager.GetLogger(typeof(UpdateProfileAppService));

        private const string LOG_CTX_GET_PROFILE = "UpdateProfileAppService.GetProfile";
        private const string LOG_CTX_UPDATE_PROFILE = "UpdateProfileAppService.UpdateUserProfile";
        private const string LOG_CTX_DELETE_PROFILE = "UpdateProfileAppService.DeleteProfile";

        private readonly WcfCallExecutor wcfCallExecutor; 

        public UpdateProfileAppService(WcfCallExecutor wcfCallExecutor)
        {
            this.wcfCallExecutor = wcfCallExecutor ?? 
                throw new ArgumentNullException(nameof(wcfCallExecutor));
        }

        public Task<WcfCallResult<GetProfileResponse>> GetProfileAsync(GetProfileRequest request)
        {
            return wcfCallExecutor.CallAsync<UpdateProfileServiceClient, GetProfileResponse>(
                () => new UpdateProfileServiceClient(WcfEndpointNames.UPDATE_SERVICE),
                c => c.GetProfileAsync(request),
                Logger,
                LOG_CTX_GET_PROFILE);

        }

        public Task<WcfCallResult<UpdateProfileResponse>> UpdateProfileAsync(UpdateProfileRequest request)
        {
            return wcfCallExecutor.CallAsync<UpdateProfileServiceClient, UpdateProfileResponse>(
                () => new UpdateProfileServiceClient(WcfEndpointNames.UPDATE_SERVICE),
                c => c.UpdateUserProfileAsync(request),
                Logger, 
                LOG_CTX_UPDATE_PROFILE);
        }

        public Task<WcfCallResult<BasicResponse>> DeleteProfileAsync(DeleteProfileRequest request)
        {
            return wcfCallExecutor.CallAsync<UpdateProfileServiceClient, BasicResponse>(
                () => new UpdateProfileServiceClient(WcfEndpointNames.UPDATE_SERVICE),
                c => c.DeleteUserProfileAsync(request),
                Logger,
                LOG_CTX_DELETE_PROFILE);
        }
    }
}