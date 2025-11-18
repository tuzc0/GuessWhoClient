using GuessWhoClient.UserServiceRef;
using System;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GuessWhoClient
{
    public partial class VerifyEmailWindow : Window
    {
        private readonly long accountId;
        private readonly string email;
        private readonly UserServiceClient client;
        private readonly DispatcherTimer cooldownTimer = new DispatcherTimer();
        private DateTime cooldownUntil;

        private static readonly Regex SixDigits = new Regex(@"^\d{6}$", RegexOptions.Compiled);

        public VerifyEmailWindow(long accountId, string email, UserServiceClient client)
        {
            InitializeComponent();
            this.accountId = accountId;
            this.email = email;
            this.client = client;

            txtInfo.Text = string.Format(GetLocalizedText("UiVerificationSentFmt"), this.email);
            txtCode.PreviewTextInput += TxtCode_PreviewTextInput;
            DataObject.AddPastingHandler(txtCode, TxtCode_OnPasting);
            txtCode.Focus();

            cooldownTimer.Interval = TimeSpan.FromSeconds(1);
            cooldownTimer.Tick += CooldownTimer_Tick;
        }

        private async void BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            var code = (txtCode.Text ?? string.Empty).Trim();

            if (!SixDigits.IsMatch(code))
            {
                ShowWarn(GetLocalizedText("UiValidationSixDigits"));
                txtCode.Focus();
                return;
            }

            btnVerify.IsEnabled = false;
            btnResend.IsEnabled = false;

            try
            {
                var resp = await client.ConfirmEmailAddressWithVerificationCodeAsync(
                    new VerifyEmailRequest { AccountId = accountId, Code = code });

                if (resp.Success)
                {
                    ShowInfo(GetLocalizedText("UiVerificationSuccess"));
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowWarn(GetLocalizedText("FaultInvalidOrExpiredCode"));
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                string key = $"Fault.{ex.Detail.Code}";
                string text = LocalOrFallback(key, ex.Detail.Message, "FaultUnexpected");
                ShowWarn(text);
            }
            catch (TimeoutException)
            {
                ShowWarn(GetLocalizedText("FaultDatabaseTimeout"));
            }
            catch (CommunicationException ex)
            {
                ShowError(GetLocalizedText("UiCommsGeneric") + "\n\n" + ex.Message);
            }
            catch (Exception ex)
            {
                ShowError(GetLocalizedText("FaultUnexpected") + "\n\n" + ex.Message);
            }
            finally
            {
                btnVerify.IsEnabled = true;
                btnResend.IsEnabled = CanResend();
            }
        }

        private async void BtnResend_Click(object sender, RoutedEventArgs e)
        {
            btnResend.IsEnabled = false;

            try
            {
                await client.ResendEmailVerificationCodeAsync(
                    new ResendVerificationRequest { AccountId = accountId });

                cooldownUntil = DateTime.UtcNow.AddSeconds(60);
                cooldownTimer.Start();
                UpdateStatus();
            }
            catch (FaultException<ServiceFault> ex)
            {
                string key = $"Fault.{ex.Detail.Code}";
                string text = LocalOrFallback(key, ex.Detail.Message, "FaultUnexpected");
                ShowWarn(text);
                btnResend.IsEnabled = CanResend();
            }
            catch (TimeoutException)
            {
                ShowWarn(GetLocalizedText("FaultDatabaseTimeout"));
                btnResend.IsEnabled = CanResend();
            }
            catch (CommunicationException ex)
            {
                ShowError(GetLocalizedText("UiCommsGeneric") + "\n\n" + ex.Message);
                btnResend.IsEnabled = CanResend();
            }
            catch (Exception ex)
            {
                ShowError(GetLocalizedText("FaultUnexpected") + "\n\n" + ex.Message);
                btnResend.IsEnabled = CanResend();
            }
        }

        private static bool AllowsNextCodeInput(TextBox codeTextBox, string incomingText)
        {

            if (incomingText == null)
            {
                incomingText = string.Empty;
            }

            if (incomingText.Any(c => !char.IsDigit(c)))
            {
                return false;
            }

            int selectionStart = codeTextBox.SelectionStart;
            int selectionLength = codeTextBox.SelectionLength;

            string proposedText = (codeTextBox.Text ?? string.Empty)
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, incomingText);

            return proposedText.Length <= 6 && proposedText.All(char.IsDigit);
        }

        private void TxtCode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var codeTextBox = (TextBox)sender;
            e.Handled = !AllowsNextCodeInput(codeTextBox, e.Text);
        }

        private void TxtCode_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            var codeTextBox = (TextBox)sender;
            var pastedText = e.DataObject.GetData(typeof(string)) as string ?? string.Empty;

            if (!AllowsNextCodeInput(codeTextBox, pastedText))
            {
                e.CancelCommand();
            }
        }

        private void CooldownTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.UtcNow >= cooldownUntil)
            {
                cooldownTimer.Stop();
                txtStatus.Text = "";
                btnResend.IsEnabled = true;
            }
            else
            {
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            var remaining = cooldownUntil - DateTime.UtcNow;

            if (remaining.TotalSeconds > 0)
            {
                txtStatus.Text = string.Format(GetLocalizedText("UiResendInFmt"), Math.Ceiling(remaining.TotalSeconds));
            }
        }

        private bool CanResend() => !cooldownTimer.IsEnabled;

        private static void ShowWarn(string message) =>
            MessageBox.Show(message, GetLocalizedText("UiTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);

        private static void ShowInfo(string message) =>
            MessageBox.Show(message, GetLocalizedText("UiTitleInfo"), MessageBoxButton.OK, MessageBoxImage.Information);

        private static void ShowError(string message) =>
            MessageBox.Show(message, GetLocalizedText("UiTitleError"), MessageBoxButton.OK, MessageBoxImage.Error);

        private static string GetLocalizedText(string key) =>
            Globalization.LocalizationProvider.Instance[key];

        private static string LocalOrFallback(string key, string serverMessage, string fallbackKey)
        {
            var text = GetLocalizedText(key);
            if (!string.IsNullOrWhiteSpace(serverMessage)) return serverMessage;
            return GetLocalizedText(fallbackKey);
        }
    }
}