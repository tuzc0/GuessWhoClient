using System;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GuessWhoClient.Alerts;
using GuessWhoClient.Globalization;
using GuessWhoClient.UserServiceRef;
using GuessWhoClient.Utilities;
using GuessWhoClient.Windows;
using log4net;

namespace GuessWhoClient
{
    public partial class VerifyEmailWindow : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(VerifyEmailWindow));

        private const int TIMER_INTERVAL_SECONDS = 1;
        private const int COOLDOWN_SECONDS = 60;
        private const int VERIFICATION_CODE_MAX_LENGTH = 6;
        private const int REGEX_TIMEOUT_MILLISECONDS = 250;

        private const string VERIFICATION_CODE_REGEX_PATTERN = @"^\d{6}$";

        private const string UI_VERIFICATION_SENT_FMT_KEY = "UiVerificationSentFmt";
        private const string UI_VERIFICATION_SUCCESS_KEY = "UIVerificationSuccess";
        private const string UI_VALIDATION_SIX_DIGITS_KEY = "UiValidationSixDigits";
        private const string FAULT_INVALID_OR_EXPIRED_CODE_KEY = "FaultInvalidOrExpiredCode";
        private const string FAULT_UNEXPECTED_KEY = "FaultUnexpected";
        private const string FAULT_DATABASE_TIMEOUT_KEY = "FaultDatabaseTimeout";
        private const string UI_COMMS_GENERIC_KEY = "UiCommsGeneric";
        private const string UI_MAIN_WINDOW_NOT_FOUND_KEY = "UiMainWindowNotFound";
        private const string UI_RESEND_IN_FMT_KEY = "UiResendInFmt";
        private const string FAULT_KEY_PREFIX = "Fault.";

        private const string LOG_VERIFY_STARTED = "Starting email verification for AccountId={0}.";
        private const string LOG_VERIFY_SUCCESS = "Email verification succeeded for AccountId={0}.";
        private const string LOG_VERIFY_FAILED_INVALID_CODE = 
            "Email verification failed due to invalid or expired code for AccountId={0}.";
        private const string LOG_RESEND_STARTED = "Resend verification code requested for AccountId={0}.";
        private const string LOG_RESEND_SUCCESS = "Verification code resent successfully for AccountId={0}.";
        private const string LOG_RESEND_COOLDOWN_STARTED =
            "Cooldown started for resend operation. AccountId={0}, CooldownSeconds={1}.";
        private const string LOG_WINDOW_UNLOADED =
            "VerifyEmailWindow unloaded. Disposing UserServiceClient for AccountId={0}.";

        private readonly long accountId;
        private readonly string email;
        private readonly UserServiceClient client;
        private readonly DispatcherTimer cooldownTimer = new DispatcherTimer();

        private readonly IAlertService alertService = new MessageBoxAlertService();
        private readonly ILocalizationService localizationService = new LocalizationService();

        private DateTime cooldownUntilUtc;
        private bool isClientClosed;

        private static readonly Regex VerificationCodeRegex =
            new Regex(VERIFICATION_CODE_REGEX_PATTERN,
                RegexOptions.Compiled, TimeSpan.FromMilliseconds(REGEX_TIMEOUT_MILLISECONDS));

        public VerifyEmailWindow(long accountId, string email, UserServiceClient client)
        {
            InitializeComponent();

            this.accountId = accountId;
            this.email = email;
            this.client = client;

            string template = localizationService.Get(UI_VERIFICATION_SENT_FMT_KEY);
            txtInfo.Text = string.Format(template, email);

            txtCode.PreviewTextInput += TxtCode_PreviewTextInput;
            DataObject.AddPastingHandler(txtCode, TxtCodeOnPasting);
            txtCode.Focus();

            cooldownTimer.Interval = TimeSpan.FromSeconds(TIMER_INTERVAL_SECONDS);
            cooldownTimer.Tick += CooldownTimerTick;
        }

        private async void BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            SetVerificationButtonsEnabled(isEnabled: false);

            Logger.InfoFormat(LOG_VERIFY_STARTED, accountId);

            try
            {
                VerifyEmailRequest request = BuildVerifyEmailRequestOrThrow();

                var response = await client.ConfirmEmailAddressWithVerificationCodeAsync(request);

                if (response.Success)
                {
                    Logger.InfoFormat(LOG_VERIFY_SUCCESS, accountId);
                    ShowInfo(GetLocalizedText(UI_VERIFICATION_SUCCESS_KEY));

                    await ServiceClientGuard.CloseSafelyAsync(client);
                    isClientClosed = true;

                    LoadLoginWindow();
                }
                else
                {
                    Logger.WarnFormat(LOG_VERIFY_FAILED_INVALID_CODE, accountId);
                    ShowWarn(GetLocalizedText(FAULT_INVALID_OR_EXPIRED_CODE_KEY));
                }
            }
            catch (InvalidOperationException ex)
            {
                ShowWarn(ex.Message);
                txtCode.Focus();
            }
            catch (FaultException<ServiceFault> ex)
            {
                string key = FAULT_KEY_PREFIX + ex.Detail.Code;
                string text = LocalOrFallback(key, ex.Detail.Message, FAULT_UNEXPECTED_KEY);

                Logger.Warn(
                    string.Format(
                        "Service fault during email verification. AccountId={0}, Code={1}.",
                        accountId, ex.Detail.Code), ex);
                
                ShowWarn(text);
            }
            catch (TimeoutException ex)
            {
                Logger.Error(
                    string.Format("Timeout while verifying email for AccountId={0}.", accountId), ex);
                
                ShowWarn(GetLocalizedText(FAULT_DATABASE_TIMEOUT_KEY));
            }
            catch (CommunicationException ex)
            {
                Logger.Error(
                    string.Format("Communication error while verifying email for AccountId={0}.", accountId), ex);
                
                string message =
                    GetLocalizedText(UI_COMMS_GENERIC_KEY) + Environment.NewLine + Environment.NewLine +
                    ex.Message;
                ShowError(message);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Unexpected error while verifying email for AccountId={0}.", accountId), ex);

                string message =
                    GetLocalizedText(FAULT_UNEXPECTED_KEY) + Environment.NewLine + Environment.NewLine + ex.Message;
                ShowError(message);
            }
            finally
            {
                SetVerificationButtonsEnabled(isEnabled: true);
            }
        }

        private async void BtnResend_Click(object sender, RoutedEventArgs e)
        {
            SetVerificationButtonsEnabled(isEnabled: false);

            Logger.InfoFormat(LOG_RESEND_STARTED, accountId);

            try
            {
                ResendVerificationRequest request = BuildResendVerificationRequest();

                await client.ResendEmailVerificationCodeAsync(request);

                Logger.InfoFormat(LOG_RESEND_SUCCESS, accountId);

                cooldownUntilUtc = DateTime.UtcNow.AddSeconds(COOLDOWN_SECONDS);
                cooldownTimer.Start();

                Logger.InfoFormat(LOG_RESEND_COOLDOWN_STARTED, accountId, COOLDOWN_SECONDS);

                UpdateStatus();
            }
            catch (FaultException<ServiceFault> ex)
            {
                string key = FAULT_KEY_PREFIX + ex.Detail.Code;
                string text = LocalOrFallback(key, ex.Detail.Message, FAULT_UNEXPECTED_KEY);

                Logger.Warn(string.Format("Service fault while resending verification code. AccountId={0}, Code={1}.",
                        accountId, ex.Detail.Code), ex);
                
                ShowWarn(text);
            }
            catch (TimeoutException ex)
            {
                Logger.Error(string.Format("Timeout while resending verification code for AccountId={0}.", accountId),
                    ex);
                
                ShowWarn(GetLocalizedText(FAULT_DATABASE_TIMEOUT_KEY));
            }
            catch (CommunicationException ex)
            {
                Logger.Error(string.Format("Communication error while resending verification code for AccountId={0}.",
                        accountId), ex);
                
                string message =
                    GetLocalizedText(UI_COMMS_GENERIC_KEY) + Environment.NewLine + Environment.NewLine +
                    ex.Message;
                ShowError(message);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    string.Format("Unexpected error while resending verification code for AccountId={0}.",accountId),
                    ex);

                string message =
                    GetLocalizedText(FAULT_UNEXPECTED_KEY) + Environment.NewLine +Environment.NewLine +
                    ex.Message;
                ShowError(message);
            }
            finally
            {
                SetVerificationButtonsEnabled(isEnabled: true);
            }
        }

        private VerifyEmailRequest BuildVerifyEmailRequestOrThrow()
        {
            string code = (txtCode.Text ?? string.Empty).Trim();

            if (!VerificationCodeRegex.IsMatch(code))
            {
                throw new InvalidOperationException(GetLocalizedText(UI_VALIDATION_SIX_DIGITS_KEY));
            }

            return new VerifyEmailRequest
            {
                AccountId = accountId,
                Code = code
            };
        }

        private ResendVerificationRequest BuildResendVerificationRequest()
        {
            return new ResendVerificationRequest
            {
                AccountId = accountId
            };
        }

        private void SetVerificationButtonsEnabled(bool isEnabled)
        {
            btnVerify.IsEnabled = isEnabled;

            if (isEnabled)
            {
                btnResend.IsEnabled = CanResend();
            }
            else
            {
                btnResend.IsEnabled = false;
            }
        }

        private void LoadLoginWindow()
        {
            var gameWindow = Window.GetWindow(this) as GameWindow;

            if (gameWindow == null)
            {
                Logger.Error("GameWindow not found when trying to load LoginWindow from VerifyEmailWindow.");
                ShowError(GetLocalizedText(UI_MAIN_WINDOW_NOT_FOUND_KEY));
                return;
            }

            gameWindow.LoadLoginWindow();
        }

        private static bool AllowsNextCodeInput(TextBox codeTextBox, string incomingText)
        {
            string safeIncomingText = incomingText ?? string.Empty;

            if (safeIncomingText.Any(character => !char.IsDigit(character)))
            {
                return false;
            }

            int selectionStart = codeTextBox.SelectionStart;
            int selectionLength = codeTextBox.SelectionLength;

            string currentText = codeTextBox.Text ?? string.Empty;

            string proposedText = currentText
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, safeIncomingText);

            return proposedText.Length <= VERIFICATION_CODE_MAX_LENGTH &&
                   proposedText.All(char.IsDigit);
        }

        private void TxtCode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var codeTextBox = (TextBox)sender;
            e.Handled = !AllowsNextCodeInput(codeTextBox, e.Text);
        }

        private void TxtCodeOnPasting(object sender, DataObjectPastingEventArgs e)
        {
            var codeTextBox = (TextBox)sender;
            string pastedText = e.DataObject.GetData(typeof(string)) as string ?? string.Empty;

            if (!AllowsNextCodeInput(codeTextBox, pastedText))
            {
                e.CancelCommand();
            }
        }

        private void CooldownTimerTick(object sender, EventArgs e)
        {
            if (DateTime.UtcNow >= cooldownUntilUtc)
            {
                cooldownTimer.Stop();
                txtStatus.Text = string.Empty;

                SetVerificationButtonsEnabled(isEnabled: true);
            }
            else
            {
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            TimeSpan remaining = cooldownUntilUtc - DateTime.UtcNow;

            if (remaining.TotalSeconds > 0)
            {
                txtStatus.Text = string.Format(
                    GetLocalizedText(UI_RESEND_IN_FMT_KEY),
                    Math.Ceiling(remaining.TotalSeconds));
            }
        }

        private bool CanResend()
        {
            return !cooldownTimer.IsEnabled;
        }

        private async void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            cooldownTimer.Stop();

            if (!isClientClosed)
            {
                Logger.InfoFormat(LOG_WINDOW_UNLOADED, accountId);

                await ServiceClientGuard.CloseSafelyAsync(client);
                isClientClosed = true;
            }
        }

        private void ShowWarn(string message)
        {
            alertService.Warn(message);
        }

        private void ShowInfo(string message)
        {
            alertService.Info(message);
        }

        private void ShowError(string message)
        {
            alertService.Error(message);
        }

        private string GetLocalizedText(string key)
        {
            return localizationService.Get(key);
        }

        private string LocalOrFallback(string key, string serverMessage, string fallbackKey)
        {
            if (!string.IsNullOrWhiteSpace(serverMessage))
            {
                return serverMessage;
            }

            return GetLocalizedText(fallbackKey);
        }
    }
}


