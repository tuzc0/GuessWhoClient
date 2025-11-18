namespace GuessWhoClient.Alerts
{
    public interface IAlertService
    {
        void Warn(string message);
        void Info(string message);
        void Error(string message);
    }
}
