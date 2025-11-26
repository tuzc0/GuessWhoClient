using GuessWhoClient.Dtos;
using GuessWhoClient.InputValidation;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class ChangePasswordDialog : UserControl
    {
        private const string VALIDATION_TITLE = "Validation";

        public event EventHandler<bool> PasswordChangeDialogClosed;

        public string CurrentPassword => pbCurrent?.Password ?? string.Empty;
        public string NewPassword => pbNewPassword?.Password ?? string.Empty;
        public string ConfirmNewPassword => pbConfirmPassword?.Password ?? string.Empty;

        public PasswordChangeRequest PasswordChangeRequest { get; private set; }

        public ChangePasswordDialog()
        {
            InitializeComponent();

            btnOk.Click += OnOkButtonClick;
            btnCancel.Click += OnCancelButtonClick;
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            var validationErrors = AccountValidator.ValidatePasswordChange(
                NewPassword,
                ConfirmNewPassword);

            if (validationErrors.Any())
            {
                MessageBox.Show(
                    string.Join(Environment.NewLine, validationErrors),
                    VALIDATION_TITLE,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PasswordChangeRequest = null;
                return;
            }

            PasswordChangeRequest = new PasswordChangeRequest(
                CurrentPassword,
                NewPassword,
                ConfirmNewPassword);

            PasswordChangeDialogClosed?.Invoke(this, true);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            PasswordChangeRequest = null;
            PasswordChangeDialogClosed?.Invoke(this, false);
        }
    }
}
