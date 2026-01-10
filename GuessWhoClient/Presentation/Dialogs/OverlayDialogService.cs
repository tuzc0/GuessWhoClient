using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewModels.Auth;
using GuessWhoClient.Presentation.Views.Auth;
using GuessWhoClient.Presentation.Views.Profile;
using GuessWhoClient.Presentation.Views.UserControls;
using GuessWhoCore.Validation.ValidationDTOs;
using log4net;
using System;

namespace GuessWhoClient.Presentation.Dialogs
{
    public interface IOverlayDialogService
    {
        void ShowChooseAvatar(string currentAvatarId, Action<string> onSelected);
        void ShowChangePassword(Action<PasswordChangeDraft> onConfirmed);
        void Close();
    }

    public sealed class OverlayDialogService : IOverlayDialogService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OverlayDialogService));
        private const string LOG_CTX = "OverlayDialogService";

        private readonly IGameScreenHost host;
        private readonly Func<ChooseAvatarView> chooseAvatarViewFactory;
        private readonly Func<ChangePasswordView> changePasswordViewFactory;

        public OverlayDialogService(
            IGameScreenHost host,
            Func<ChooseAvatarView> chooseAvatarViewFactory,
            Func<ChangePasswordView> changePasswordViewFactory)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
            this.chooseAvatarViewFactory = chooseAvatarViewFactory ?? 
                throw new ArgumentNullException(nameof(chooseAvatarViewFactory));
            this.changePasswordViewFactory = changePasswordViewFactory ??
                throw new ArgumentNullException(nameof(changePasswordViewFactory));
        }

        public void ShowChooseAvatar(string currentAvatarId, Action<string> onSelected)
        {
            try
            {
                var view = chooseAvatarViewFactory.Invoke();

                if (view.DataContext is ViewModels.Profile.ChooseAvatarViewModel viewModel)
                {
                    viewModel.Initialize(currentAvatarId);
                    viewModel.AvatarSelected = onSelected;
                    viewModel.RequestClose = Close;
                }

                host.SetOverlayContent(view);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(LOG_CTX, ex);
            }
        }

        public void ShowChangePassword(Action<PasswordChangeDraft> onConfirmed)
        {
            try
            {
                var view = changePasswordViewFactory.Invoke();
                if (view.DataContext is ChangePasswordViewModel viewModel)
                {
                    viewModel.PasswordConfirmed = onConfirmed;
                    viewModel.RequestClose = Close;
                }

                host.SetOverlayContent(view);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(LOG_CTX, ex);
            }
        }

        public void Close()
        {
            host.SetOverlayContent(null);
        }
    }
}
