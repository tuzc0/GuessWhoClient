using GuessWhoClient.Presentation.Dialogs;
using GuessWhoClient.Services.Alerts;
using System;

namespace GuessWhoClient.Presentation.Services.Alerts
{
    public sealed class GameAlertService : IAlertService
    {
        private const string EMPTY = "";

        private readonly IGameMessageDialogService dialogService;

        public GameAlertService(IGameMessageDialogService dialogService)
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        public void Ok(string message, string title)
        {
            dialogService.Show(title ?? EMPTY, message ?? EMPTY);
        }

        public void Warn(string message, string title)
        {
            dialogService.Show(title ?? EMPTY, message ?? EMPTY);
        }

        public void Info(string message, string title)
        {
            dialogService.Show(title ?? EMPTY, message ?? EMPTY);
        }

        public void Error(string message, string title)
        {
            dialogService.Show(title ?? EMPTY, message ?? EMPTY);
        }

        public void Error(string v)
        {
            dialogService.Show(EMPTY, v ?? EMPTY);
        }
    }
}
