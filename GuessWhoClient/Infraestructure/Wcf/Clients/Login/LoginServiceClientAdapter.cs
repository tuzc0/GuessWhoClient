using GuessWhoClient.LoginServiceRef;
using GuessWhoCore.Contracts.Faults;
using GuessWhoCore.Contracts.Request;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients.Login
{
    internal sealed class LoginServiceClientSessionAdapter : ILoginServiceClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginServiceClientSessionAdapter));

        private const string LOG_CTX_LOGIN = "LoginServiceClientSessionAdapter.LoginUserAsync";
        private const string LOG_CTX_TOUCH = "LoginServiceClientSessionAdapter.TouchPresenceAsync";
        private const string LOG_CTX_LOGOUT = "LoginServiceClientSessionAdapter.LogoutUserAsync";

        private const string EMPTY = "";

        private readonly object syncRoot = new object();

        private LoginServiceClient loginClient;

        public Task<WcfCallResult<LoginResponse>> LoginUserAsync(LoginRequest request)
        {
            if (request == null)
            {
                return Task.FromResult(WcfCallResult<LoginResponse>.Fail(LoginFaultKeys.CODE_REQUEST_NULL, EMPTY));
            }

            return CallLoginAsync(request);
        }

        public Task<WcfCallResult<BasicResponse>> TouchPresenceAsync(TouchPresenceRequest request)
        {
            if (request == null)
            {
                return Task.FromResult(WcfCallResult<BasicResponse>.Fail(LoginFaultKeys.CODE_REQUEST_NULL, EMPTY));
            }

            return CallTouchAsync(request);
        }

        public Task<WcfCallResult<BasicResponse>> LogoutUserAsync(LogoutRequest request)
        {
            if (request == null)
            {
                return Task.FromResult(WcfCallResult<BasicResponse>.Fail(LoginFaultKeys.CODE_REQUEST_NULL, EMPTY));
            }

            return CallLogoutAsync(request);
        }

        private async Task<WcfCallResult<LoginResponse>> CallLoginAsync(LoginRequest request)
        {
            try
            {
                LoginServiceClient client = EnsureClient();

                var proxyRequest = new LoginRequest
                {
                    Email = request.Email ?? EMPTY,
                    Password = request.Password ?? EMPTY
                };

                LoginResponse proxyResponse = await client.LoginUserAsync(proxyRequest);

                if (proxyResponse == null)
                {
                    return WcfCallResult<LoginResponse>.Ok(new LoginResponse
                    {
                        UserId = 0,
                        DisplayName = EMPTY,
                        Email = EMPTY,
                        ValidUser = false
                    });
                }

                return WcfCallResult<LoginResponse>.Ok(new LoginResponse
                {
                    UserId = proxyResponse.UserId,
                    DisplayName = proxyResponse.DisplayName ?? EMPTY,
                    Email = proxyResponse.Email ?? EMPTY,
                    ValidUser = proxyResponse.ValidUser
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn(LOG_CTX_LOGIN, ex);
                return WcfCallResult<LoginResponse>.Fail(ex.Detail?.Code ?? EMPTY, ex.Detail?.MessageKey ?? EMPTY);
            }
            catch (EndpointNotFoundException ex)
            {
                Logger.Error(LOG_CTX_LOGIN, ex);
                ResetClient();
                return WcfCallResult<LoginResponse>.Fail(WcfTechnicalFaultCodes.ENDPOINT_NOT_FOUND, EMPTY);
            }
            catch (CommunicationException ex)
            {
                Logger.Error(LOG_CTX_LOGIN, ex);
                ResetClient();
                return WcfCallResult<LoginResponse>.Fail(WcfTechnicalFaultCodes.COMMUNICATION_ERROR, EMPTY);
            }
            catch (TimeoutException ex)
            {
                Logger.Error(LOG_CTX_LOGIN, ex);
                ResetClient();
                return WcfCallResult<LoginResponse>.Fail(WcfTechnicalFaultCodes.TIMEOUT, EMPTY);
            }
        }

        private async Task<WcfCallResult<BasicResponse>> CallTouchAsync(TouchPresenceRequest request)
        {
            try
            {
                LoginServiceClient client = EnsureClient();

                BasicResponse proxyResponse = await client.TouchPresenceAsync(request);

                if (proxyResponse == null)
                {
                    return WcfCallResult<BasicResponse>.Ok(new BasicResponse
                    {
                        Success = false,
                        Code = EMPTY,
                        MeesageKey = EMPTY
                    });
                }

                return WcfCallResult<BasicResponse>.Ok(new BasicResponse
                {
                    Success = proxyResponse.Success,
                    Code = proxyResponse.Code ?? EMPTY,
                    MeesageKey = proxyResponse.MeesageKey ?? EMPTY
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn(LOG_CTX_TOUCH, ex);
                return WcfCallResult<BasicResponse>.Fail(ex.Detail?.Code ?? EMPTY, ex.Detail?.MessageKey ?? EMPTY);
            }
            catch (EndpointNotFoundException ex)
            {
                Logger.Error(LOG_CTX_TOUCH, ex);
                ResetClient();
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.ENDPOINT_NOT_FOUND, EMPTY);
            }
            catch (CommunicationException ex)
            {
                Logger.Error(LOG_CTX_TOUCH, ex);
                ResetClient();
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.COMMUNICATION_ERROR, EMPTY);
            }
            catch (TimeoutException ex)
            {
                Logger.Error(LOG_CTX_TOUCH, ex);
                ResetClient();
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.TIMEOUT, EMPTY);
            }
        }

        private async Task<WcfCallResult<BasicResponse>> CallLogoutAsync(LogoutRequest request)
        {
            try
            {
                LoginServiceClient client = EnsureClient();

                BasicResponse proxyResponse = await client.LogoutUserAsync(request);

                SafeCloseClient();

                if (proxyResponse == null)
                {
                    return WcfCallResult<BasicResponse>.Ok(new BasicResponse
                    {
                        Success = false,
                        Code = EMPTY,
                        MeesageKey = EMPTY
                    });
                }

                return WcfCallResult<BasicResponse>.Ok(new BasicResponse
                {
                    Success = proxyResponse.Success,
                    Code = proxyResponse.Code ?? EMPTY,
                    MeesageKey = proxyResponse.MeesageKey ?? EMPTY
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Warn(LOG_CTX_LOGOUT, ex);
                SafeCloseClient();
                return WcfCallResult<BasicResponse>.Fail(ex.Detail?.Code ?? EMPTY, ex.Detail?.MessageKey ?? EMPTY);
            }
            catch (EndpointNotFoundException ex)
            {
                Logger.Error(LOG_CTX_LOGOUT, ex);
                ResetClient();
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.ENDPOINT_NOT_FOUND, EMPTY);
            }
            catch (CommunicationException ex)
            {
                Logger.Error(LOG_CTX_LOGOUT, ex);
                ResetClient();
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.COMMUNICATION_ERROR, EMPTY);
            }
            catch (TimeoutException ex)
            {
                Logger.Error(LOG_CTX_LOGOUT, ex);
                ResetClient();
                return WcfCallResult<BasicResponse>.Fail(WcfTechnicalFaultCodes.TIMEOUT, EMPTY);
            }
        }

        private LoginServiceClient EnsureClient()
        {
            lock (syncRoot)
            {
                if (loginClient != null && IsClientUsable(loginClient))
                {
                    return loginClient;
                }

                SafeCloseClient();

                loginClient = new LoginServiceClient(WcfEndpointNames.LOGIN_SERVICE);

                ICommunicationObject comm = loginClient;

                if (comm.State == CommunicationState.Created)
                {
                    comm.Open();
                }

                return loginClient;
            }
        }

        private static bool IsClientUsable(LoginServiceClient client)
        {
            ICommunicationObject comm = client;
            return comm.State == CommunicationState.Opened || comm.State == CommunicationState.Opening;
        }

        private void ResetClient()
        {
            lock (syncRoot)
            {
                SafeCloseClient();
            }
        }

        private void SafeCloseClient()
        {
            if (loginClient == null)
            {
                return;
            }

            ICommunicationObject comm = loginClient;

            try
            {
                if (comm.State == CommunicationState.Faulted)
                {
                    comm.Abort();
                }
                else
                {
                    comm.Close();
                }
            }
            catch (CommunicationException)
            {
                comm.Abort();
            }
            catch (TimeoutException)
            {
                comm.Abort();
            }
            finally
            {
                loginClient = null;
            }
        }
    }
}