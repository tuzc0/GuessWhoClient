using GuessWhoClient.UpdateServiceRef;
using GuessWhoClient.Presentation.Infrastructure;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoCore.Contracts.Faults;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients
{
    internal sealed class UpdateProfileServiceClientAdapter : IUpdateProfileServiceClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateProfileServiceClientAdapter));
        private readonly IUiFaultMapper _faultMapper;

        public UpdateProfileServiceClientAdapter(IUiFaultMapper faultMapper)
        {
            _faultMapper = faultMapper ?? throw new ArgumentNullException(nameof(faultMapper));
        }

        public async Task<WcfCallResult<GetProfileResponse>> GetProfileAsync(GetProfileRequest request)
        {
            UpdateProfileServiceClient client = null;
            try
            {
                client = new UpdateProfileServiceClient(WcfEndpointNames.UPDATE_SERVICE);
                var response = await client.GetProfileAsync(request);
                return WcfCallResult<GetProfileResponse>.Ok(response);
            }
            catch (Exception ex) { return HandleException<GetProfileResponse>(ex, nameof(GetProfileAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<bool>> UpdateUserProfileAsync(UpdateProfileRequest request)
        {
            UpdateProfileServiceClient client = null;
            try
            {
                client = new UpdateProfileServiceClient(WcfEndpointNames.UPDATE_SERVICE);
                var response = await client.UpdateUserProfileAsync(request);
                return WcfCallResult<bool>.Ok(response != null && response.Updated);
            }
            catch (Exception ex) { return HandleException<bool>(ex, nameof(UpdateUserProfileAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        public async Task<WcfCallResult<bool>> DeleteUserProfileAsync(DeleteProfileRequest request)
        {
            UpdateProfileServiceClient client = null;
            try
            {
                client = new UpdateProfileServiceClient(WcfEndpointNames.UPDATE_SERVICE);
                var response = await client.DeleteUserProfileAsync(request);
                return WcfCallResult<bool>.Ok(response != null && response.Success);
            }
            catch (Exception ex) { return HandleException<bool>(ex, nameof(DeleteUserProfileAsync)); }
            finally { await ServiceClientGuard.CloseSafelyAsync(client); }
        }

        private WcfCallResult<T> HandleException<T>(Exception ex, string context)
        {
            if (ex is FaultException<ServiceFault> faultEx)
            {
                Logger.Warn(context, faultEx);
                var mapping = _faultMapper.Map(faultEx.Detail?.Code);
                return WcfCallResult<T>.Fail(mapping.UiKey, faultEx.ToString());
            }
            Logger.Error(context, ex);
            return WcfCallResult<T>.Fail("WCF_UNEXPECTED_ERROR", ex.Message);
        }

        public void Dispose() { }
    }
}