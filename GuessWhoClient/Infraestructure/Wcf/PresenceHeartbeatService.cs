using GuessWhoClient.Infraestructure.Session;
using GuessWhoClient.Infraestructure.Wcf.Clients.Login;
using GuessWhoCore.Contracts.Requests;
using log4net;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace GuessWhoClient.Infraestructure.Wcf
{
    internal sealed class PresenceHeartbeatService : IPresenceHeartbeatService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PresenceHeartbeatService));

        private const string LOG_CTX_TICK = "PresenceHeartbeatService.Tick";
        private const int MIN_INTERVAL_SECONDS = 5;
        private const long NO_USER = 0;

        private readonly ILoginServiceClient loginServiceClient;
        private readonly int intervalSeconds;

        private readonly SemaphoreSlim tickLock = new SemaphoreSlim(1, 1);

        private Timer timer;
        private long currentUserId;

        public PresenceHeartbeatService(ILoginServiceClient loginServiceClient, int intervalSeconds)
        {
            this.loginServiceClient = loginServiceClient ?? throw new ArgumentNullException(nameof(loginServiceClient));
            this.intervalSeconds = Math.Max(MIN_INTERVAL_SECONDS, intervalSeconds);
        }

        public void Start(long userId)
        {
            currentUserId = userId;

            timer?.Dispose();
            timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));
        }

        public void Stop()
        {
            currentUserId = NO_USER;

            timer?.Dispose();
            timer = null;
        }

        public void Dispose()
        {
            Stop();
            tickLock.Dispose();
        }

        private void OnTick(object state)
        {
            _ = TickSafeAsync();
        }

        private async Task TickSafeAsync()
        {
            long userId = currentUserId;

            if (userId <= 0)
            {
                return;
            }

            bool entered = false;

            try
            {
                entered = await tickLock.WaitAsync(0);

                if (!entered)
                {
                    return;
                }

                await loginServiceClient.TouchPresenceAsync(new TouchPresenceRequest
                {
                    UserId = userId
                });
            }
            catch (FaultException ex)
            {
                Logger.Warn(LOG_CTX_TICK, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                Logger.Warn(LOG_CTX_TICK, ex);
            }
            catch (CommunicationException ex)
            {
                Logger.Warn(LOG_CTX_TICK, ex);
            }
            catch (TimeoutException ex)
            {
                Logger.Warn(LOG_CTX_TICK, ex);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn(LOG_CTX_TICK, ex);
            }
            finally
            {
                if (entered)
                {
                    tickLock.Release();
                }
            }
        }
    }
}
