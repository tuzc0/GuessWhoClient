using GuessWhoClient.Application.Services.Account;
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Wcf;
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
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GuessWhoClient.ViewModels.Profile
{
    public sealed class CreateAccountViewModel : AuthViewModelBase, INotifyDataErrorInfo
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CreateAccountViewModel));

        private const string LOG_CTX_CREATE = "CreateAccountViewModel.CreateAccount";

        private const string KEY_UI_ACCOUNT_CREATED_FMT = "UiAccountCreatedForFmt";

        private readonly IUserAppService userAppService;
        private readonly IGameScreenManager gameScreenManager;
        private readonly AccountFlowContext accountFlowContext;
        private readonly ICreateAccountValidationErrorsMapper errorsMapper;

        private readonly DataErrors dataErrors;

        private string email;
        private string displayName;
        private string password;
        private string confirmPassword;
        private bool isPasswordVisible;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected override ILog LoggerInstance => Logger;

        public CreateAccountViewModel(
            IUserAppService userAppService,
            IAlertService alertService,
            ILocalizationService localizationService,
            IUiFaultMapper faultMapper,
            ICreateAccountValidationErrorsMapper errorsMapper,
            IGameScreenManager gameScreenManager,
            AccountFlowContext accountFlowContext)
            : base(alertService, localizationService, faultMapper)
        {
            this.userAppService = userAppService ?? 
                throw new ArgumentNullException(nameof(userAppService));
            this.errorsMapper = errorsMapper ?? 
                throw new ArgumentNullException(nameof(errorsMapper));
            this.gameScreenManager = gameScreenManager ?? 
                throw new ArgumentNullException(nameof(gameScreenManager));
            this.accountFlowContext = accountFlowContext ?? 
                throw new ArgumentNullException(nameof(accountFlowContext));

            dataErrors = new DataErrors();
            dataErrors.ErrorsChanged += OnInternalErrorsChanged;

            email = EMPTY;
            displayName = EMPTY;
            password = EMPTY;
            confirmPassword = EMPTY;

            CreateAccountCommand = new AsyncRelayCommand(CreateAccountAsync, CanExecuteCreate);
            BackCommand = new AsyncRelayCommand(BackAsync, CanExecuteBack);

            Validate();
        }

        public AsyncRelayCommand CreateAccountCommand { get; }
        public AsyncRelayCommand BackCommand { get; }

        public bool HasErrors => dataErrors.HasErrors;

        public bool IsPasswordVisible
        {
            get => isPasswordVisible;
            set => SetProperty(ref isPasswordVisible, value);
        }

        public string Email
        {
            get => email;
            set
            {
                if (!SetProperty(ref email, value ?? EMPTY))
                {
                    return;
                }

                Validate();
            }
        }

        public string DisplayName
        {
            get => displayName;
            set
            {
                if (!SetProperty(ref displayName, value ?? EMPTY))
                {
                    return;
                }

                Validate();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                if (!SetProperty(ref password, value ?? EMPTY))
                {
                    return;
                }

                Validate();
            }
        }

        public string ConfirmPassword
        {
            get => confirmPassword;
            set
            {
                if (!SetProperty(ref confirmPassword, value ?? EMPTY))
                {
                    return;
                }

                Validate();
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return dataErrors.GetErrors(propertyName);
        }

        public void Validate()
        {
            var passwordsDraft = 
                new PasswordConfirmationDraft(Password ?? EMPTY, ConfirmPassword ?? EMPTY);
            var draft = new UserRulesDraft(Email ?? EMPTY, DisplayName ?? EMPTY, passwordsDraft);

            var errors = UserRules.Validate(draft);
            var mapped = errorsMapper.Map(errors);

            dataErrors.ReplaceAllErrors(mapped);

            OnPropertyChanged(nameof(HasErrors));
            RaiseCommandsCanExecuteChanged();
        }

        protected override void OnIsBusyChanged(string propertyName)
        {
            RaiseCommandsCanExecuteChanged();
        }

        private void OnInternalErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            ErrorsChanged?.Invoke(this, e);
            OnPropertyChanged(nameof(HasErrors));
            RaiseCommandsCanExecuteChanged();
        }

        private bool CanExecuteCreate()
        {
            return !IsBusy && !HasErrors;
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
                if (!TryValidateBeforeSubmit())
                {
                    return;
                }

                var request = new RegisterRequest
                {
                    Email = (Email ?? EMPTY).Trim(),
                    DisplayName = DisplayName ?? EMPTY,
                    Password = Password ?? EMPTY
                };

                WcfCallResult<RegisterResponse> result = await userAppService.RegisterUserAsync(request);

                if (result == null || !result.IsSuccess || !result.HasValue || result.Value == null)
                {
                    ShowCallError(result, KEY_UI_GENERIC_ERROR, LOG_CTX_CREATE);
                    return;
                }

                RegisterResponse response = result.Value;

                string message = FormatFromResource(KEY_UI_ACCOUNT_CREATED_FMT, request.Email);

                alertService.Info(message, localizationService.Get(KEY_UI_TITLE_INFO));

                if (response.EmailVerificationRequired)
                {
                    accountFlowContext.SetPendingEmailVerification(response.AccountId, response.Email);
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

        private bool TryValidateBeforeSubmit()
        {
            Validate();

            if (!HasErrors)
            {
                return true;
            }

            string first = dataErrors.FirstErrorOrEmpty(nameof(Email));

            if (string.IsNullOrWhiteSpace(first))
            {
                first = dataErrors.FirstErrorOrEmpty(nameof(DisplayName));
            }

            if (string.IsNullOrWhiteSpace(first))
            {
                first = dataErrors.FirstErrorOrEmpty(nameof(Password));
            }

            if (string.IsNullOrWhiteSpace(first))
            {
                first = dataErrors.FirstErrorOrEmpty(nameof(ConfirmPassword));
            }

            if (!string.IsNullOrWhiteSpace(first))
            {
                alertService.Warn(first, localizationService.Get(KEY_UI_TITLE_WARNING));
            }

            return false;
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

