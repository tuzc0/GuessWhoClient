using GuessWhoClient.Application.Services;
using GuessWhoClient.Domain.Models;
using GuessWhoClient.Domain.Validation;
using GuessWhoClient.Presentation.ViewsModels.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GuessWhoClient.ViewModels
{
    public sealed class CreateAccountViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private static readonly IReadOnlyDictionary<string, string> ValidationKeyToPropertyName =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "Registration.Email.Required", nameof(Email) },
                { "Registration.Email.InvalidFormat", nameof(Email) },
                { "Registration.Email.TooLong", nameof(Email) },
                
                { "Registration.DisplayName.Required", nameof(DisplayName) },
                { "Registration.DisplayName.TooShort", nameof(DisplayName) },      
                { "Registration.DisplayName.TooLong", nameof(DisplayName) },
                { "Registration.DisplayName.InvalidChars", nameof(DisplayName) },
                
                { "Registration.Password.Required", nameof(Password) },
                { "Registration.Password.TooShort", nameof(Password) },           
                { "Registration.Password.TooLong", nameof(Password) },
                
                { "Registration.ConfirmPassword.Required", nameof(ConfirmPassword) },
                { "Registration.ConfirmPassword.Mismatch", nameof(ConfirmPassword) }
    };


        private readonly IValidationMessageMapper validationMessageMapper;
        private readonly IUserAppService userAppService;
        private readonly DataErrors dataErrors;

        private string email;
        private string displayName;
        private string password;
        private string confirmPassword;

        private bool isPasswordVisible;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public CreateAccountViewModel(IValidationMessageMapper validationMessageMapper, IUserAppService userAppService)
        {
            this.validationMessageMapper = validationMessageMapper
                ?? throw new ArgumentNullException(nameof(validationMessageMapper));
            this.userAppService = userAppService
                ?? throw new ArgumentNullException(nameof(userAppService));

            dataErrors = new DataErrors();
            dataErrors.ErrorsChanged += OnDataErrorsChanged;

            email = string.Empty;
            displayName = string.Empty;
            password = string.Empty;
            confirmPassword = string.Empty;

            CreateAccountCommand = new AsyncRelayCommand(CreateAccountAsync, CanCreateAccount);

            Validate();
        }

        public ICommand CreateAccountCommand { get; }

        public bool HasErrors => dataErrors.HasErrors;
        public bool CanInteract => !HasErrors;

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
                if (!SetProperty(ref email, value ?? string.Empty))
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
                if (!SetProperty(ref displayName, value ?? string.Empty))
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
                if (!SetProperty(ref password, value ?? string.Empty))
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
                if (!SetProperty(ref confirmPassword, value ?? string.Empty))
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
            var mappedErrors = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

            var userDraft = new UserRegistrationDraft(
                Email,
                DisplayName,
                Password,
                ConfirmPassword);

            IReadOnlyList<ValidationError> validationErrors = UserRegistrationRules.Validate(userDraft);

            for (int index = 0; index < validationErrors.Count; index++)
            {
                ValidationError validationError = validationErrors[index];

                if (validationError == null)
                {
                    continue;
                }

                string propertyName = ResolvePropertyName(validationError.Key);

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    continue;
                }

                string message = validationMessageMapper.ToMessage(validationError.Key);

                if (!mappedErrors.TryGetValue(propertyName, out IReadOnlyList<string> existing))
                {
                    mappedErrors[propertyName] = new List<string> { message };
                    continue;
                }

                var list = existing as List<string> ?? new List<string>(existing);

                if (!list.Contains(message))
                {
                    list.Add(message);
                }

                mappedErrors[propertyName] = list;
            }

            dataErrors.ReplaceAllErrors(mappedErrors);

            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(CanInteract));
            RaiseCreateAccountCanExecuteChanged();
        }

        private static string ResolvePropertyName(string validationKey)
        {
            if (string.IsNullOrWhiteSpace(validationKey))
            {
                return string.Empty;
            }

            return ValidationKeyToPropertyName.TryGetValue(validationKey, out string propertyName)
                ? propertyName
                : string.Empty;
        }

        private void OnDataErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            ErrorsChanged?.Invoke(this, e);

            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(CanInteract));

            RaiseCreateAccountCanExecuteChanged();
        }

        private bool CanCreateAccount()
        {
            return CanInteract;
        }

        private async Task CreateAccountAsync()
        {
            Validate();

            if (HasErrors)
            {
                return;
            }

            try
            {
                var input = new UserRegistrationInput(
                    displayName, 
                    password,
                    email
                );

                var result = await userAppService.RegisterAsync(input);

                MessageBox.Show($"¡Cuenta creada! ID: {result.AccountId}. Verifica tu correo.");

                // Aquí podrías navegar al Login o cerrar la ventana
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Error al crear cuenta: {ex.Message}");
            }
        }

        private void RaiseCreateAccountCanExecuteChanged()
        {
            if (CreateAccountCommand is AsyncRelayCommand asyncCommand)
            {
                asyncCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
