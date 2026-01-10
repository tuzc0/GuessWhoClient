using GuessWhoClient.Domain.Validation;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoCore.Validation;
using GuessWhoCore.Validation.ValidationDTOs;
using System;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public sealed class ChangePasswordViewModel : ViewModelBase
    {
        private const string EMPTY = "";
        private const string KEY_UI_TITLE_WARNING = "UiTitleWarning";
        private const string KEY_UI_GENERIC_ERROR = "UiGenericError";

        private readonly IAlertService alertService;
        private readonly ILocalizationService localizationService;
        private readonly IUiFaultMapper uiFaultMapper;
        private readonly IValidationIssueMapper validationIssueMapper;

        private string currentPassword;
        private string newPassword;
        private string confirmPassword;
        private bool isPasswordsVisible;

        public ChangePasswordViewModel(
            IAlertService alertService,
            ILocalizationService localizationService,
            IUiFaultMapper uiFaultMapper,
            IValidationIssueMapper validationIssueMapper)
        {
            this.alertService = alertService ??
                throw new ArgumentNullException(nameof(alertService));
            this.localizationService = localizationService ??
                throw new ArgumentNullException(nameof(localizationService));
            this.uiFaultMapper = uiFaultMapper ??
                throw new ArgumentNullException(nameof(uiFaultMapper));
            this.validationIssueMapper = validationIssueMapper ??
                throw new ArgumentNullException(nameof(validationIssueMapper));

            currentPassword = EMPTY;
            newPassword = EMPTY;
            confirmPassword = EMPTY;

            OkCommand = new RelayCommand(Ok, CanExecuteCommands);
            CancelCommand = new RelayCommand(Cancel, CanExecuteCommands);
        }

        public string CurrentPassword
        {
            get => currentPassword;
            set => SetProperty(ref currentPassword, value ?? EMPTY);
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

        public bool IsPasswordsVisible
        {
            get => isPasswordsVisible;
            set => SetProperty(ref isPasswordsVisible, value);
        }

        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action<PasswordChangeDraft> PasswordConfirmed { get; set; }
        public Action RequestClose { get; set; }

        private void Ok()
        {
            string safeCurrent = CurrentPassword ?? EMPTY;
            string safeNew = NewPassword ?? EMPTY;
            string safeConfirm = ConfirmPassword ?? EMPTY;

            if (string.IsNullOrWhiteSpace(safeCurrent))
            {
                WarnValidationCode(UserValidationCodes.CURRENT_PASSWORD_REQUIRED);
                return;
            }

            if (string.IsNullOrWhiteSpace(safeNew))
            {
                WarnValidationCode(UserValidationCodes.PASSWORD_REQUIRED);
                return;
            }

            if (string.IsNullOrWhiteSpace(safeConfirm))
            {
                WarnValidationCode(UserValidationCodes.CONFIRM_PASSWORD_REQUIRED);
                return;
            }

            if (!string.Equals(safeNew, safeConfirm, StringComparison.Ordinal))
            {
                WarnValidationCode(UserValidationCodes.CONFIRM_PASSWORD_MISMATCH);
                return;
            }

            if (!ValidateAgainstRulesOrWarn(safeCurrent, safeNew))
            {
                return;
            }

            PasswordConfirmed?.Invoke(new PasswordChangeDraft(safeCurrent, safeNew));
            RequestClose?.Invoke();
        }

        private bool ValidateAgainstRulesOrWarn(string safeCurrent, string safeNew)
        {
            var passwordDraft = new PasswordChangeDraft(safeCurrent, safeNew);

            var profileDraft = new ProfileUpdateDraft(
                displayName: EMPTY,
                avatarId: EMPTY,
                passwordChange: passwordDraft);

            var errors = UserRules.Validate(profileDraft);

            if (errors == null || errors.Count == 0)
            {
                return true;
            }

            string code = (errors[0].Key ?? EMPTY).Trim();

            if (string.IsNullOrWhiteSpace(code))
            {
                WarnUiKey(KEY_UI_GENERIC_ERROR);
                return false;
            }

            WarnValidationCode(code);
            return false;
        }

        private void Cancel()
        {
            RequestClose?.Invoke();
        }

        private void WarnValidationCode(string validationCode)
        {
            string uiKey = ResolveValidationUiKey(validationCode);

            if (string.IsNullOrWhiteSpace(uiKey))
            {
                uiKey = KEY_UI_GENERIC_ERROR;
            }

            WarnUiKey(uiKey);
        }

        private string ResolveValidationUiKey(string code)
        {
            string safeCode = (code ?? EMPTY).Trim();

            if (string.IsNullOrWhiteSpace(safeCode))
            {
                return EMPTY;
            }

            ValidationIssueMapping mapping = validationIssueMapper.Map(safeCode);

            if (mapping.IsMapped && !string.IsNullOrWhiteSpace(mapping.MessageKey))
            {
                return (mapping.MessageKey ?? EMPTY).Trim();
            }

            UiKeyMapping fallback = uiFaultMapper.Map(safeCode);

            return fallback.IsMapped
                ? (fallback.UiKey ?? EMPTY).Trim()
                : EMPTY;
        }

        private void WarnUiKey(string uiKey)
        {
            string safeKey = (uiKey ?? EMPTY).Trim();

            if (string.IsNullOrWhiteSpace(safeKey))
            {
                safeKey = KEY_UI_GENERIC_ERROR;
            }

            string title = localizationService.Get(KEY_UI_TITLE_WARNING) ?? EMPTY;
            string message = localizationService.Get(safeKey) ?? EMPTY;

            if (string.IsNullOrWhiteSpace(message))
            {
                message = localizationService.Get(KEY_UI_GENERIC_ERROR) ?? EMPTY;
            }

            alertService.Warn(message, title);
        }

        private bool CanExecuteCommands() => !IsBusy;

        protected override void OnIsBusyChanged(string propertyName)
        {
            OkCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
