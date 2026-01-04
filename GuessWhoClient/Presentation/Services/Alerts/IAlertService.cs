namespace GuessWhoClient.Services.Alerts
{
    public interface IAlertService
    {
        void Warn(string message);
        void Info(string message);
        void Error(string message);

        void Warn(string message, string title);
        void Info(string message, string title);
        void Error(string message, string title);
    }
}
