namespace GuessWhoClient.Presentation.Dialogs
{
    public interface IGameConfirmDialogService
    {
        bool Confirm(string title, string message, string confirmButtonText, string cancelButtonText);
    }
}
