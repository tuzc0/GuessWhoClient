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
using GuessWhoCore.Validation;
using GuessWhoCore.Validation.ValidationDTOs;
using log4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public sealed class RecoverPasswordViewModel : AuthViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RecoverPasswordViewModel));

        private const string LOG_CTX_SEND = "RecoverPasswordViewModel.SendCode";
        private const string LOG_CTX_RESEND = "RecoverPasswordViewModel.ResendCode";
        private const string LOG_CTX_UPDATE = "RecoverPasswordViewModel.UpdatePassword";

        private const string KEY_UI_EMAIL_REQUIRED = "UiValidationEmailRequired";
        private const string KEY_UI_EMAIL_INVALID = "UiValidationEmailInvalid";

        private const string KEY_UI_PASSWORD_REQUIRED = "UiValidationPasswordRequired";
        private const string KEY_UI_PASSWORD_INVALID = "UiValidationPasswordInvalid";
        private const string KEY_UI_CONFIRM_PASSWORD_REQUIRED = "UiValidationConfirmPasswordRequired";
        private const string KEY_UI_PASSWORD_DONT_MATCH = "UiValidationPasswordDontMatch";

        private const string KEY_UI_CODE_INVALID = "UIVerificationCodeInvalid";

        private const string KEY_UI_RECOVERY_AMBIGUOUS = "UiPasswordRecoveryAmbiguousSuccess";
        private const string KEY_UI_PASSWORD_UPDATED = "UiPasswordUpdated";

        private const int CODE_LENGTH = 6;

        private const string DUMMY_VALID_PASSWORD_FOR_EMAIL_VALIDATION = "Aa1!aaaa";
        private const string DUMMY_VALID_EMAIL_FOR_PASSWORD_RULES = "player@example.com";
        private const string DUMMY_VALID_DISPLAY_NAME_FOR_PASSWORD_RULES = "Player";

        private readonly IUserAppService userAppService;
        private readonly IGameScreenManager gameScreenManager;

        private string email;
        private string code;
        private string newPassword;
        private string confirmPassword;

        private bool isPasswordVisible;
        private bool isEmailStepVisible;
        private bool isPasswordStepVisible;

        protected override ILog LoggerInstance => Logger;

        public RecoverPasswordViewModel(
            IUserAppService userAppService,
            IAlertService alertService,
            ILocalizationService localizationService,
            IUiFaultMapper faultMapper,
            IGameScreenManager gameScreenManager)
            : base(alertService, localizationService, faultMapper)
        {
            this.userAppService = userAppService ??
                throw new ArgumentNullException(nameof(userAppService));
            this.gameScreenManager = gameScreenManager ??
                throw new ArgumentNullException(nameof(gameScreenManager));

            email = EMPTY;
            code = EMPTY;
            newPassword = EMPTY;
            confirmPassword = EMPTY;

            isEmailStepVisible = true;
            isPasswordStepVisible = false;

            SendCodeCommand = new AsyncRelayCommand(SendCodeAsync, CanExecuteCommands);
            ResendCodeCommand = new AsyncRelayCommand(ResendCodeAsync, CanExecuteCommands);
            UpdatePasswordCommand = new AsyncRelayCommand(UpdatePasswordAsync, CanExecuteCommands);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanExecuteCommands);
        }

        public AsyncRelayCommand SendCodeCommand { get; }
        public AsyncRelayCommand ResendCodeCommand { get; }
        public AsyncRelayCommand UpdatePasswordCommand { get; }
        public AsyncRelayCommand CancelCommand { get; }

        public string Email
        {
            get => email;
            set => SetProperty(ref email, value ?? EMPTY);
        }

        public string Code
        {
            get => code;
            set => SetProperty(ref code, value ?? EMPTY);
        }

        public string NewPassword
        {
            get => newPassword;
            set => SetProperty(ref newPassword, value ?? EMPTY);
        }

        public string ConfirmPassword
        {
            get => confirmPassword;
            set => SetProperty(ref confirmPassword, value ?? EMPTY);
        }

        public bool IsPasswordVisible
        {
            get => isPasswordVisible;
            set => SetProperty(ref isPasswordVisible, value);
        }

        public bool IsEmailStepVisible
        {
            get => isEmailStepVisible;
            private set => SetProperty(ref isEmailStepVisible, value);
        }

        public bool IsPasswordStepVisible
        {
            get => isPasswordStepVisible;
            private set => SetProperty(ref isPasswordStepVisible, value);
        }

        protected override void OnIsBusyChanged(string propertyName)
        {
            SendCodeCommand.RaiseCanExecuteChanged();
            ResendCodeCommand.RaiseCanExecuteChanged();
            UpdatePasswordCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCommands()
        {
            return !IsBusy;
        }

        private Task SendCodeAsync()
        {
            return SendOrResendCodeAsync(moveToPasswordStep: true, logContext: LOG_CTX_SEND);
        }

        private Task ResendCodeAsync()
        {
            return SendOrResendCodeAsync(moveToPasswordStep: false, logContext: LOG_CTX_RESEND);
        }

        private async Task SendOrResendCodeAsync(bool moveToPasswordStep, string logContext)
        {
            if (!TryBeginOperation())
            {
                return;
            }

            try
            {
                string safeEmail = (Email ?? EMPTY).Trim();

                if (!TryValidateEmailRequiredAndFormat(safeEmail))
                {
                    return;
                }

                var request = new PasswordRecoveryRequest
                {
                    Email = safeEmail
                };

                WcfCallResult<PasswordRecoveryResponse> result =
                    await userAppService.SendPasswordRecoveryCodeAsync(request);

                if (result == null || !result.IsSuccess || !result.HasValue)
                {
                    ShowCallError(result, KEY_UI_GENERIC_ERROR, logContext);
                    return;
                }

                alertService.Info(
                    localizationService.Get(KEY_UI_RECOVERY_AMBIGUOUS),
                    localizationService.Get(KEY_UI_TITLE_INFO));

                if (moveToPasswordStep)
                {
                    IsEmailStepVisible = false;
                    IsPasswordStepVisible = true;
                }
            }
            finally
            {
                EndOperation();
            }
        }

        private async Task UpdatePasswordAsync()
        {
            if (!TryBeginOperation())
            {
                return;
            }

            try
            {
                if (!TryValidateUpdatePassword())
                {
                    return;
                }

                UpdatePasswordRequest request = CreateUpdatePasswordRequest();

                WcfCallResult<bool> result = await userAppService.UpdatePasswordAsync(request);

                if (result == null || !result.IsSuccess || !result.HasValue || !result.Value)
                {
                    ShowUpdatePasswordError(result);
                    return;
                }

                alertService.Info(
                    localizationService.Get(KEY_UI_PASSWORD_UPDATED),
                    localizationService.Get(KEY_UI_TITLE_INFO));

                await CancelAsync();
            }
            finally
            {
                EndOperation();
            }
        }

        private bool TryValidateEmailRequiredAndFormat(string safeEmail)
        {
            if (string.IsNullOrWhiteSpace(safeEmail))
            {
                alertService.Warn(
                    localizationService.Get(KEY_UI_EMAIL_REQUIRED),
                    localizationService.Get(KEY_UI_TITLE_WARNING));
                return false;
            }

            if (!IsEmailFormatValidByUserRules(safeEmail))
            {
                alertService.Warn(
                    localizationService.Get(KEY_UI_EMAIL_INVALID),
                    localizationService.Get(KEY_UI_TITLE_WARNING));
                return false;
            }

            return true;
        }

        private static bool IsEmailFormatValidByUserRules(string safeEmail)
        {
            var draft = new LoginDraft(safeEmail, DUMMY_VALID_PASSWORD_FOR_EMAIL_VALIDATION);
            IReadOnlyList<ValidationError> errors = UserRules.ValidateLogin(draft);

            return errors == null || errors.Count == 0;
        }

        private bool TryValidateUpdatePassword()
        {
            string safeEmail = (Email ?? EMPTY).Trim();
            string safeCode = (Code ?? EMPTY).Trim();
            string safeNew = NewPassword ?? EMPTY;
            string safeConfirm = ConfirmPassword ?? EMPTY;

            if (string.IsNullOrWhiteSpace(safeEmail))
            {
                alertService.Warn(
                    localizationService.Get(KEY_UI_EMAIL_REQUIRED),
                    localizationService.Get(KEY_UI_TITLE_WARNING));
                return false;
            }

            if (!IsValidCode(safeCode))
            {
                alertService.Warn(
                    localizationService.Get(KEY_UI_CODE_INVALID),
                    localizationService.Get(KEY_UI_TITLE_WARNING));
                return false;
            }

            return TryValidateNewPasswordByUserRules(safeNew, safeConfirm);
        }

        private bool TryValidateNewPasswordByUserRules(string newPasswordValue, string confirmPasswordValue)
        {
            var passwords = new PasswordConfirmationDraft(
                newPasswordValue ?? EMPTY,
                confirmPasswordValue ?? EMPTY);

            var draft = new UserRulesDraft(
                DUMMY_VALID_EMAIL_FOR_PASSWORD_RULES,
                DUMMY_VALID_DISPLAY_NAME_FOR_PASSWORD_RULES,
                passwords);

            IReadOnlyList<ValidationError> errors = UserRules.Validate(draft);

            if (errors == null || errors.Count == 0)
            {
                return true;
            }

            string uiKey = ResolvePasswordValidationUiKey(errors);

            alertService.Warn(
                localizationService.Get(uiKey),
                localizationService.Get(KEY_UI_TITLE_WARNING));

            return false;
        }

        private string ResolvePasswordValidationUiKey(IReadOnlyList<ValidationError> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return KEY_UI_PASSWORD_INVALID;
            }

            for (int index = 0; index < errors.Count; index++)
            {
                string codeValue = errors[index]?.Code ?? EMPTY;

                if (string.Equals(codeValue, UserValidationCodes.PASSWORD_REQUIRED, StringComparison.Ordinal))
                {
                    return KEY_UI_PASSWORD_REQUIRED;
                }

                if (string.Equals(codeValue, UserValidationCodes.CONFIRM_PASSWORD_REQUIRED, StringComparison.Ordinal))
                {
                    return KEY_UI_CONFIRM_PASSWORD_REQUIRED;
                }

                if (string.Equals(codeValue, UserValidationCodes.CONFIRM_PASSWORD_MISMATCH, StringComparison.Ordinal))
                {
                    return KEY_UI_PASSWORD_DONT_MATCH;
                }

                if (string.Equals(codeValue, UserValidationCodes.PASSWORD_TOO_SHORT, StringComparison.Ordinal) ||
                    string.Equals(codeValue, UserValidationCodes.PASSWORD_TOO_LONG, StringComparison.Ordinal) ||
                    string.Equals(codeValue, UserValidationCodes.PASSWORD_INVALID_FORMAT, StringComparison.Ordinal))
                {
                    return KEY_UI_PASSWORD_INVALID;
                }
            }

            return KEY_UI_PASSWORD_INVALID;
        }

        private UpdatePasswordRequest CreateUpdatePasswordRequest()
        {
            return new UpdatePasswordRequest
            {
                Email = (Email ?? EMPTY).Trim(),
                VerificationCode = (Code ?? EMPTY).Trim(),
                NewPassword = NewPassword ?? EMPTY
            };
        }

        private void ShowUpdatePasswordError(WcfCallResult<bool> result)
        {
            if (result != null &&
                (string.Equals(result.FaultCode, PasswordRecoveryFaultKeys.CODE_CODE_EXPIRED, StringComparison.Ordinal) ||
                 string.Equals(result.FaultCode, PasswordRecoveryFaultKeys.CODE_CODE_INVALID, StringComparison.Ordinal)))
            {
                ShowCallWarning(result, KEY_UI_GENERIC_ERROR, LOG_CTX_UPDATE);
                return;
            }

            ShowCallError(result, KEY_UI_GENERIC_ERROR, LOG_CTX_UPDATE);
        }

        private Task CancelAsync()
        {
            Reset();
            gameScreenManager.HideOverlay();
            return Task.CompletedTask;
        }

        private void Reset()
        {
            Email = EMPTY;
            Code = EMPTY;
            NewPassword = EMPTY;
            ConfirmPassword = EMPTY;

            IsEmailStepVisible = true;
            IsPasswordStepVisible = false;

            IsPasswordVisible = false;
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
    }
}
