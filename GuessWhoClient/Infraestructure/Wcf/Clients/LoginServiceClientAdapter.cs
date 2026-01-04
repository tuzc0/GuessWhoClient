using GuessWhoClient.LoginServiceRef;
using GuessWhoClient.Presentation.Infrastructure;
using GuessWhoCore.Contracts.Faults;
using GuessWhoCore.Contracts.Request;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients
{
    internal sealed class LoginServiceClientAdapter : ILoginServiceClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginServiceClientAdapter));

        private const string LOG_CTX_LOGIN = "LoginServiceClientAdapter.LoginUserAsync";
        private const string LOG_CTX_LOGOUT = "LoginServiceClientAdapter.LogoutUserAsync";

        private const string CODE_ENDPOINT_NOT_FOUND = "WCF_ENDPOINT_NOT_FOUND";
        private const string CODE_SECURITY = "WCF_SECURITY_ERROR";
        private const string CODE_TIMEOUT = "WCF_TIMEOUT";
        private const string CODE_COMMUNICATION = "WCF_COMMUNICATION_ERROR";
        private const string CODE_UNEXPECTED = "WCF_UNEXPECTED_ERROR";

        public async Task<WcfCallResult<LoginResponse>> LoginUserAsync(LoginRequest request)
        {
            LoginServiceClient client = null;

            try
            {
                client = new LoginServiceClient(WcfEndpointNames.LOGIN_SERVICE);

                var proxyRequest = new LoginRequest
                {
                    Email = request?.Email,
                    Password = request?.Password
                };

                LoginResponse proxyResponse = await client.LoginUserAsync(proxyRequest);

                var coreResponse = new LoginResponse
                {
                    UserId = proxyResponse.UserId,
                    DisplayName = proxyResponse.DisplayName,
                    Email = proxyResponse.Email,
                    ValidUser = proxyResponse.ValidUser
                };

                return WcfCallResult<LoginResponse>.Ok(coreResponse);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn(LOG_CTX_LOGIN, ex);

                string code = ex.Detail?.Code;
                string message = ex.ToString();

                return WcfCallResult<LoginResponse>.Fail(code, message);
            }
            catch (EndpointNotFoundException ex)
            {
                Logger.Error(LOG_CTX_LOGIN, ex);
                return WcfCallResult<LoginResponse>.Fail(CODE_ENDPOINT_NOT_FOUND, null);
            }
            catch (MessageSecurityException ex)
            {
                Logger.Error(LOG_CTX_LOGIN, ex);
                return WcfCallResult<LoginResponse>.Fail(CODE_SECURITY, null);
            }
            catch (TimeoutException ex)
            {
                Logger.Error(LOG_CTX_LOGIN, ex);
                return WcfCallResult<LoginResponse>.Fail(CODE_TIMEOUT, null);
            }
            catch (CommunicationException ex)
            {
                Logger.Error(LOG_CTX_LOGIN, ex);
                return WcfCallResult<LoginResponse>.Fail(CODE_COMMUNICATION, null);
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_LOGIN, ex);
                return WcfCallResult<LoginResponse>.Fail(CODE_UNEXPECTED, ex.Message);
            }
            finally
            {
                await ServiceClientGuard.CloseSafelyAsync(client);
            }
        }

        public async Task<WcfCallResult<BasicResponse>> LogoutUserAsync(LogoutRequest request)
        {
            LoginServiceClient client = null;

            try
            {
                client = new LoginServiceClient(WcfEndpointNames.LOGIN_SERVICE);

                var proxyRequest = new LogoutRequest
                {
                    UserProfileId = request.UserProfileId
                };

                BasicResponse proxyResponse = await client.LogoutUserAsync(proxyRequest);

                var coreResponse = new BasicResponse
                {
                    Success = proxyResponse.Success,
                    Code = proxyResponse.Code,
                    MeesageKey = proxyResponse.MeesageKey
                };

                return WcfCallResult<BasicResponse>.Ok(coreResponse);
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn(LOG_CTX_LOGOUT, ex);

                string code = ex.Detail?.Code;
                string message = ex.ToString();

                return WcfCallResult<BasicResponse>.Fail(code, message);
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_LOGOUT, ex);
                return WcfCallResult<BasicResponse>.Fail(CODE_UNEXPECTED, ex.Message);
            }
            finally
            {
                await ServiceClientGuard.CloseSafelyAsync(client);
            }
        }
    }
}