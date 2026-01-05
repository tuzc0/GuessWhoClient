using GuessWhoClient.Application.Services.Account;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoCore.Contracts.Faults;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public sealed class VerifyEmailViewModel : AuthViewModelBase, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(VerifyEmailViewModel));

        private const string LOG_CTX_VERIFY = "VerifyEmailViewModel.Verify";
        private const string LOG_CTX_RESEND = "VerifyEmailViewModel.Resend";

        private const string KEY_UI_VERIFICATION_SENT_FMT = "UiVerificationSentFmt";
        private const string KEY_UI_VERIFICATION_SUCCESS = "UIVerificationSuccess";
        private const string KEY_UI_RESEND_IN_FMT = "UiResendInFmt";
        private const string KEY_UI_VERIFICATION_RESENT = "UIVerificationResent";
        private const string KEY_UI_VALIDATION_SIX_DIGITS = "UiValidationSixDigits";

        private const string KEY_UI_CODE_INVALID = "UIVerificationCodeInvalid";

        private const int COOLDOWN_SECONDS = 60;
        private const int CODE_LENGTH = 6;

        private readonly IUserAppService userAppService;
        private readonly IGameScreenManager gameScreenManager;
        private readonly AccountFlowContext accountFlowContext;

        private readonly DispatcherTimer cooldownTimer;

        private string code;
        private string infoText;
        private string statusText;

        private DateTime cooldownUntilUtc;

        protected override ILog LoggerInstance => Logger;

        public VerifyEmailViewModel(
            IUserAppService userAppService,
            IAlertService alertService,
            ILocalizationService localizationService,
            IUiFaultMapper faultMapper,
            IGameScreenManager gameScreenManager,
            AccountFlowContext accountFlowContext)
            : base(alertService, localizationService, faultMapper)
        {
            this.userAppService = userAppService ?? 
                throw new ArgumentNullException(nameof(userAppService));
            this.gameScreenManager = gameScreenManager ?? 
                throw new ArgumentNullException(nameof(gameScreenManager));
            this.accountFlowContext = accountFlowContext ??
                throw new ArgumentNullException(nameof(accountFlowContext));

            code = EMPTY;

            cooldownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            cooldownTimer.Tick += CooldownTick;

            VerifyCommand = new AsyncRelayCommand(VerifyAsync, CanExecuteCommands);
            ResendCommand = new AsyncRelayCommand(ResendAsync, CanExecuteResend);
            BackCommand = new AsyncRelayCommand(BackAsync, CanExecuteBack);

            RefreshInfoText();
        }

        public AsyncRelayCommand VerifyCommand { get; }
        public AsyncRelayCommand ResendCommand { get; }
        public AsyncRelayCommand BackCommand { get; }

        public string Code
        {
            get => code;
            set => SetProperty(ref code, value ?? EMPTY);
        }

        public string InfoText
        {
            get => infoText;
            private set => SetProperty(ref infoText, value ?? EMPTY);
        }

        public string StatusText
        {
            get => statusText;
            private set => SetProperty(ref statusText, value ?? EMPTY);
        }

        protected override void OnIsBusyChanged(string propertyName)
        {
            VerifyCommand.RaiseCanExecuteChanged();
            ResendCommand.RaiseCanExecuteChanged();
            BackCommand.RaiseCanExecuteChanged();
        }

        public void RefreshInfoText()
        {
            string email = accountFlowContext.PendingVerificationEmail ?? EMPTY;
            InfoText = FormatFromResource(KEY_UI_VERIFICATION_SENT_FMT, email);
        }

        private bool CanExecuteCommands()
        {
            return !IsBusy;
        }

        private bool CanExecuteResend()
        {
            return !IsBusy && !cooldownTimer.IsEnabled;
        }

        private bool CanExecuteBack()
        {
            return !IsBusy;
        }

        private async Task VerifyAsync()
        {
            if (!TryBeginOperation())
            {
                return;
            }

            try
            {
                if (!IsValidCode(Code))
                {
                    alertService.Warn(localizationService.Get(
                        KEY_UI_VALIDATION_SIX_DIGITS), 
                        localizationService.Get(KEY_UI_TITLE_WARNING));
                    return;
                }

                long accountId = accountFlowContext.PendingVerificationAccountId;

                if (accountId <= 0)
                {
                    alertService.Error(localizationService.Get(
                        KEY_UI_GENERIC_ERROR), 
                        localizationService.Get(KEY_UI_TITLE_ERROR));
                    return;
                }

                var request = new VerifyEmailRequest
                {
                    AccountId = accountId,
                    Code = (Code ?? EMPTY).Trim()
                };

                WcfCallResult<VerifyEmailResponse> result = 
                    await userAppService.VerifyEmailAsync(request);

                if (result == null || !result.IsSuccess || !result.HasValue || result.Value == null)
                {
                    ShowVerifyError(result);
                    return;
                }

                if (result.Value.Success)
                {
                    alertService.Info(localizationService.Get(
                        KEY_UI_VERIFICATION_SUCCESS), 
                        localizationService.Get(KEY_UI_TITLE_INFO));

                    accountFlowContext.ClearPendingEmailVerification();
                    gameScreenManager.HideOverlay();
                    gameScreenManager.ShowScreen(GameScreenType.Login);
                    return;
                }

                alertService.Warn(localizationService.Get(
                    KEY_UI_CODE_INVALID), 
                    localizationService.Get(KEY_UI_TITLE_WARNING));
            }
            finally
            {
                EndOperation();
            }
        }

        private void ShowVerifyError(WcfCallResult<VerifyEmailResponse> result)
        {
            if (result != null &&
                (string.Equals(result.FaultCode, EmailVerificationFaultKeys.CODE_CODE_EXPIRED, StringComparison.Ordinal) ||
                 string.Equals(result.FaultCode, EmailVerificationFaultKeys.CODE_CODE_INCORRECT, StringComparison.Ordinal) ||
                 string.Equals(result.FaultCode, EmailVerificationFaultKeys.CODE_CODE_ALREADY_USED, StringComparison.Ordinal) ||
                 string.Equals(result.FaultCode, EmailVerificationFaultKeys.CODE_CODE_INVALID_FORMAT, StringComparison.Ordinal) ||
                 string.Equals(result.FaultCode, EmailVerificationFaultKeys.CODE_CODE_MISSING, StringComparison.Ordinal)))
            {
                ShowCallWarning(result, KEY_UI_GENERIC_ERROR, LOG_CTX_VERIFY);
                return;
            }

            ShowCallError(result, KEY_UI_GENERIC_ERROR, LOG_CTX_VERIFY);
        }

        private async Task ResendAsync()
        {
            if (!TryBeginOperation())
            {
                return;
            }

            try
            {
                long accountId = accountFlowContext.PendingVerificationAccountId;

                if (accountId <= 0)
                {
                    alertService.Error(localizationService.Get(
                        KEY_UI_GENERIC_ERROR), 
                        localizationService.Get(KEY_UI_TITLE_ERROR));
                    return;
                }

                var request = new ResendVerificationRequest
                {
                    AccountId = accountId
                };

                WcfCallResult<bool> result = await userAppService.ResendVerificationCodeAsync(request);

                if (result == null || !result.IsSuccess || !result.HasValue || !result.Value)
                {
                    ShowCallError(result, KEY_UI_GENERIC_ERROR, LOG_CTX_RESEND);
                    return;
                }

                alertService.Info(localizationService.Get(
                    KEY_UI_VERIFICATION_RESENT), 
                    localizationService.Get(KEY_UI_TITLE_INFO));

                cooldownUntilUtc = DateTime.UtcNow.AddSeconds(COOLDOWN_SECONDS);
                cooldownTimer.Start();
                UpdateStatus();
            }
            finally
            {
                EndOperation();
                ResendCommand.RaiseCanExecuteChanged();
            }
        }

        private Task BackAsync()
        {
            accountFlowContext.ClearPendingEmailVerification();
            gameScreenManager.HideOverlay();
            gameScreenManager.ShowScreen(GameScreenType.Login);
            return Task.CompletedTask;
        }

        private static bool IsValidCode(string value)
        {
            string trimmed = (value ?? EMPTY).Trim();

            if (trimmed.Length != CODE_LENGTH)
            {
                return false;
            }

            for (int index = 0; index < trimmed.Length; index++)
            {
                if (!char.IsDigit(trimmed[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private void CooldownTick(object sender, EventArgs e)
        {
            if (DateTime.UtcNow >= cooldownUntilUtc)
            {
                cooldownTimer.Stop();
                StatusText = EMPTY;
                ResendCommand.RaiseCanExecuteChanged();
                return;
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            TimeSpan remaining = cooldownUntilUtc - DateTime.UtcNow;

            if (remaining.TotalSeconds <= 0)
            {
                StatusText = EMPTY;
                return;
            }

            string template = localizationService.Get(KEY_UI_RESEND_IN_FMT);

            try
            {
                StatusText = string.Format(template, Math.Ceiling(remaining.TotalSeconds));
            }
            catch (FormatException)
            {
                StatusText = template ?? EMPTY;
            }
        }

        public void Dispose()
        {
            cooldownTimer.Stop();
            cooldownTimer.Tick -= CooldownTick;
        }
    }
}

