using System;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class ChangePasswordDialog : UserControl
    {
        private const string VALIDATION_TITLE = "Validation";
        private const string ERROR_NEW_PASSWORD_EMPTY = "New password cannot be empty.";
        private const string ERROR_PASSWORD_MISMATCH = "New password and confirmation do not match.";

        public event EventHandler<bool> PasswordChangeDialogClosed;

        public string CurrentPassword => pbCurrent?.Password ?? string.Empty;
        public string NewPassword => pbNew?.Password ?? string.Empty;
        public string ConfirmNewPassword => pbConfirm?.Password ?? string.Empty;

        public ChangePasswordDialog()
        {
            InitializeComponent();

            btnOk.Click += OnOkButtonClick;
            btnCancel.Click += OnCancelButtonClick;
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                MessageBox.Show(
                    ERROR_NEW_PASSWORD_EMPTY,
                    VALIDATION_TITLE,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!string.Equals(NewPassword, ConfirmNewPassword, StringComparison.Ordinal))
            {
                MessageBox.Show(
                    ERROR_PASSWORD_MISMATCH,
                    VALIDATION_TITLE,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            PasswordChangeDialogClosed?.Invoke(this, true);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            PasswordChangeDialogClosed?.Invoke(this, false);
        }
    }
}
