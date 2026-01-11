namespace GuessWhoClient.Infraestructure.Session
{
    public interface IPresenceHeartbeatService
    {
        void Start(long userId);

        void Stop();
    }
}
