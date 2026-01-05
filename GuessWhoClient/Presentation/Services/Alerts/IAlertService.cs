namespace GuessWhoClient.Services.Alerts
{
    public interface IAlertService
    {
        void Ok(string message, string title);
        void Warn(string message, string title);
        void Info(string message, string title);
        void Error(string message, string title);
    }
}
