using GuessWhoClient.Application.Services.Account;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Presentation.Dialogs;
using GuessWhoClient.Presentation.Navegation;
using GuessWhoClient.Presentation.ViewModels.Auth;
using GuessWhoClient.Presentation.ViewModels.Profile;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using GuessWhoCore.Validation;
using GuessWhoCore.Validation.ValidationDTOs;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoClient.ViewModels.Profile
{
    public sealed class CreateAccountViewModel : AuthViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CreateAccountViewModel));

        private new const string EMPTY = "";
        private const string LOG_CTX_CREATE_ACCOUNT = "CreateAccountViewModel.CreateAccount";

        private const string KEY_UI_ACCOUNT_CREATED_FMT = "UiAccountCreatedForFmt";

        private new const string KEY_UI_TITLE_ERROR = "UiTitleError";
        private new const string KEY_UI_TITLE_WARNING = "UiTitleWarning";

        private const string KEY_UI_CREATE_INVALID_DATA_TITLE = "UiCreateAccountInvalidDataTitle";
        private const string KEY_UI_CREATE_INVALID_DATA_INTRO = "UiCreateAccountInvalidDataIntro";

        private const string BULLET_PREFIX = "• ";

        private readonly IUserAppService userAccountAppService;
        private readonly IGameScreenManager gameScreenManager;
        private readonly AccountFlowContext accountFlowContext;
        private readonly ICreateAccountValidationErrorsMapper createAccountValidationErrorsMapper;
        private readonly IGameMessageDialogService gameMessageDialogService;

        private string email;
        private string displayName;
        private string password;
        private string confirmPassword;
        private bool isPasswordVisible;

        protected override ILog LoggerInstance => Logger;

        public CreateAccountViewModel(
            IUserAppService userAppService,
            IAlertService alertService,
            ILocalizationService localizationService,
            IUiFaultMapper faultMapper,
            ICreateAccountValidationErrorsMapper errorsMapper,
            IGameMessageDialogService gameMessageDialogService,
            IGameScreenManager gameScreenManager,
            AccountFlowContext accountFlowContext)
            : base(alertService, localizationService, faultMapper)
        {
            userAccountAppService = userAppService ?? 
                throw new ArgumentNullException(nameof(userAppService));
            createAccountValidationErrorsMapper = errorsMapper ?? 
                throw new ArgumentNullException(nameof(errorsMapper));
            this.gameMessageDialogService = gameMessageDialogService ?? 
                throw new ArgumentNullException(nameof(gameMessageDialogService));
            this.gameScreenManager = gameScreenManager ?? 
                throw new ArgumentNullException(nameof(gameScreenManager));
            this.accountFlowContext = accountFlowContext ?? 
                throw new ArgumentNullException(nameof(accountFlowContext));

            email = EMPTY;
            displayName = EMPTY;
            password = EMPTY;
            confirmPassword = EMPTY;

            CreateAccountCommand = new AsyncRelayCommand(CreateAccountAsync, CanExecuteCreateAccount);
            BackCommand = new AsyncRelayCommand(BackAsync, CanExecuteBack);
        }

        public AsyncRelayCommand CreateAccountCommand { get; }
        public AsyncRelayCommand BackCommand { get; }

        public bool IsPasswordVisible
        {
            get => isPasswordVisible;
            set => SetProperty(ref isPasswordVisible, value);
        }

        public string Email
        {
            get => email;
            set => SetProperty(ref email, value ?? EMPTY);
        }

        public string DisplayName
        {
            get => displayName;
            set => SetProperty(ref displayName, value ?? EMPTY);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value ?? EMPTY);
        }

        public string ConfirmPassword
        {
            get => confirmPassword;
            set => SetProperty(ref confirmPassword, value ?? EMPTY);
        }

        protected override void OnIsBusyChanged(string propertyName)
        {
            RaiseCommandsCanExecuteChanged();
        }

        private bool CanExecuteCreateAccount()
        {
            return !IsBusy;
        }

        private bool CanExecuteBack()
        {
            return !IsBusy;
        }

        private async Task CreateAccountAsync()
        {
            if (!TryBeginOperation())
            {
                return;
            }

            try
            {
                if (!TryValidateInputOrShowDialog())
                {
                    return;
                }

                string safeEmail = (Email ?? EMPTY).Trim();
                string safeDisplayName = (DisplayName ?? EMPTY).Trim();

                var registerRequest = new RegisterRequest
                {
                    Email = safeEmail,
                    DisplayName = safeDisplayName,
                    Password = Password ?? EMPTY
                };

                WcfCallResult<RegisterResponse> registerResult =
                    await userAccountAppService.RegisterUserAsync(registerRequest);

                if (registerResult == null || !registerResult.IsSuccess || !registerResult.HasValue || registerResult.Value == null)
                {
                    ShowCallError(registerResult, KEY_UI_GENERIC_ERROR, LOG_CTX_CREATE_ACCOUNT);
                    return;
                }

                RegisterResponse registerResponse = registerResult.Value;

                string createdMessage = FormatFromResource(KEY_UI_ACCOUNT_CREATED_FMT, safeEmail);
                alertService.Info(createdMessage, localizationService.Get(KEY_UI_TITLE_INFO));

                if (registerResponse.EmailVerificationRequired)
                {
                    accountFlowContext.SetPendingEmailVerification(
                        accountId: registerResponse.AccountId,
                        email: safeEmail,
                        returnScreen: GameScreenType.Login);

                    gameScreenManager.ShowOverlay(GameScreenType.VerifyEmail);
                    return;
                }

                gameScreenManager.ShowScreen(GameScreenType.Login);
            }
            finally
            {
                EndOperation();
            }
        }

        private bool TryValidateInputOrShowDialog()
        {
            string validationMessage = BuildValidationSummaryMessageOrEmpty();

            if (string.IsNullOrWhiteSpace(validationMessage))
            {
                return true;
            }

            string dialogTitle =
                localizationService.Get(KEY_UI_CREATE_INVALID_DATA_TITLE) ??
                localizationService.Get(KEY_UI_TITLE_WARNING) ??
                localizationService.Get(KEY_UI_TITLE_ERROR) ??
                EMPTY;

            gameMessageDialogService.Show(dialogTitle, validationMessage);

            return false;
        }

        private string BuildValidationSummaryMessageOrEmpty()
        {
            var draft = new UserRulesDraft(
                (Email ?? EMPTY).Trim(),
                (DisplayName ?? EMPTY).Trim(),
                new PasswordConfirmationDraft(Password ?? EMPTY, ConfirmPassword ?? EMPTY));

            IReadOnlyList<ValidationError> validationErrors = UserRules.Validate(draft);
            IReadOnlyDictionary<string, IReadOnlyList<string>> mappedErrors =
                createAccountValidationErrorsMapper.Map(validationErrors);

            if (mappedErrors == null || mappedErrors.Count == 0)
            {
                return EMPTY;
            }

            var uniqueMessages = new HashSet<string>(StringComparer.Ordinal);
            var messageBuilder = new StringBuilder();

            string intro =
                localizationService.Get(KEY_UI_CREATE_INVALID_DATA_INTRO) ?? EMPTY;

            if (!string.IsNullOrWhiteSpace(intro))
            {
                messageBuilder.AppendLine(intro.Trim());
                messageBuilder.AppendLine();
            }

            bool anyMessageAdded = false;

            foreach (KeyValuePair<string, IReadOnlyList<string>> entry in mappedErrors)
            {
                IReadOnlyList<string> messages = entry.Value;

                if (messages == null || messages.Count == 0)
                {
                    continue;
                }

                for (int index = 0; index < messages.Count; index++)
                {
                    string message = messages[index] ?? EMPTY;

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        continue;
                    }

                    message = message.Trim();

                    if (!uniqueMessages.Add(message))
                    {
                        continue;
                    }

                    messageBuilder.Append(BULLET_PREFIX);
                    messageBuilder.AppendLine(message);

                    anyMessageAdded = true;
                }
            }

            if (!anyMessageAdded)
            {
                return EMPTY;
            }

            return messageBuilder.ToString().Trim();
        }

        private Task BackAsync()
        {
            gameScreenManager.ShowScreen(GameScreenType.Login);
            return Task.CompletedTask;
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            CreateAccountCommand.RaiseCanExecuteChanged();
            BackCommand.RaiseCanExecuteChanged();
        }
    }
}

