using GuessWhoClient.LoginServiceRef;
using GuessWhoCore.Contracts.Faults;
using GuessWhoCore.Contracts.Request;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf.Clients.Login
{
    internal sealed class LoginServiceClientAdapter : ILoginServiceClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginServiceClientAdapter));

        private const string LOG_CTX_LOGIN = "LoginServiceClientAdapter.LoginUserAsync";
        private const string LOG_CTX_LOGOUT = "LoginServiceClientAdapter.LogoutUserAsync";

        private const string EMPTY = "";

        private readonly WcfCallExecutor wcfCallExecutor;

        public LoginServiceClientAdapter(WcfCallExecutor wcfCallExecutor)
        {
            this.wcfCallExecutor = wcfCallExecutor ??
                throw new ArgumentNullException(nameof(wcfCallExecutor));
        }

        public Task<WcfCallResult<LoginResponse>> LoginUserAsync(LoginRequest request)
        {
            if (request == null)
            {
                return Task.FromResult(WcfCallResult<LoginResponse>.Fail(
                    LoginFaultKeys.CODE_REQUEST_NULL,
                    EMPTY));
            }

            return wcfCallExecutor.CallAsync<LoginServiceClient, LoginResponse>(
                clientFactory: () => new LoginServiceClient(WcfEndpointNames.LOGIN_SERVICE),
                operationAsync: async client =>
                {
                    var proxyRequest = new LoginRequest
                    {
                        Email = request.Email ?? EMPTY,
                        Password = request.Password ?? EMPTY
                    };

                    LoginResponse proxyResponse = await client.LoginUserAsync(proxyRequest);

                    if (proxyResponse == null)
                    {
                        return new LoginResponse
                        {
                            UserId = 0,
                            DisplayName = EMPTY,
                            Email = EMPTY,
                            ValidUser = false
                        };
                    }

                    return new LoginResponse
                    {
                        UserId = proxyResponse.UserId,
                        DisplayName = proxyResponse.DisplayName ?? EMPTY,
                        Email = proxyResponse.Email ?? EMPTY,
                        ValidUser = proxyResponse.ValidUser
                    };
                },
                logger: Logger,
                logContext: LOG_CTX_LOGIN);
        }

        public Task<WcfCallResult<BasicResponse>> LogoutUserAsync(LogoutRequest request)
        {
            if (request == null)
            {
                return Task.FromResult(WcfCallResult<BasicResponse>.Fail(
                    LoginFaultKeys.CODE_REQUEST_NULL,
                    EMPTY));
            }

            return wcfCallExecutor.CallAsync<LoginServiceClient, BasicResponse>(
                clientFactory: () => new LoginServiceClient(WcfEndpointNames.LOGIN_SERVICE),
                operationAsync: async client =>
                {
                    var proxyRequest = new LogoutRequest
                    {
                        UserProfileId = request.UserProfileId
                    };

                    BasicResponse proxyResponse = await client.LogoutUserAsync(proxyRequest);

                    if (proxyResponse == null)
                    {
                        return new BasicResponse
                        {
                            Success = false,
                            Code = EMPTY,
                            MeesageKey = EMPTY
                        };
                    }

                    return new BasicResponse
                    {
                        Success = proxyResponse.Success,
                        Code = proxyResponse.Code ?? EMPTY,
                        MeesageKey = proxyResponse.MeesageKey ?? EMPTY
                    };
                },
                logger: Logger,
                logContext: LOG_CTX_LOGOUT);
        }
    }
}