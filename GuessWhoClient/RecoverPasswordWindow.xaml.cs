using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using GuessWhoClient.UserServiceRef;


namespace GuessWhoClient
{
    public partial class RecoverPasswordWindow : UserControl
    {
        private bool isPasswordVisible = false;

        public RecoverPasswordWindow()
        {
            InitializeComponent();
        }

        // -----------------------------------------------------------
        // PASO 1: ENVIAR EL CÓDIGO
        // -----------------------------------------------------------
        private async void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter your email.");
                return;
            }

            btnSendCode.IsEnabled = false;

            try
            {
                using (var userService = new UserServiceClient("NetTcpBinding_IUserService"))
                {
                    var request = new PasswordRecoveryRequest { Email = email };

                    // Llamada al método 1: Solicitar código
                    var response = await userService.SendPasswordRecoveryCodeAsync(request);

                    if (response.Success)
                    {
                        MessageBox.Show(response.Message);

                        // Ocultamos paso 1, mostramos paso 2
                        pnlEmailStep.Visibility = Visibility.Collapsed;
                        pnlPasswordStep.Visibility = Visibility.Visible;

                        txtEmail.IsEnabled = false;
                    }
                    else
                    {
                        MessageBox.Show(response.Message);
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending code: " + ex.Message);
            }
            finally
            {
                btnSendCode.IsEnabled = true;
            }
        }


        private async void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim();
            string newPass = isPasswordVisible ? txtNewPasswordVisible.Text : pwdNewPassword.Password;
            string confirmPass = isPasswordVisible ? txtConfirmPasswordVisible.Text : pwdConfirmPassword.Password;

            if (code.Length != 6)
            {
                MessageBox.Show("Verification code must be 6 digits.");
                return;
            }

            if (string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Password cannot be empty.");
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            btnChangePassword.IsEnabled = false;

            try
            {
                using (var userService = new UserServiceClient("NetTcpBinding_IUserService"))
                {
                    var request = new UpdatePasswordRequest
                    {
                        Email = txtEmail.Text.Trim(),
                        VerificationCode = code,
                        NewPassword = newPass
                    };


                    bool success = await userService.UpdatePasswordWithVerificationCodeAsync(request);

                    if (success)
                    {
                        MessageBox.Show("Password updated successfully! Please login.");

                        // Cerramos la ventana volviendo al login
                        OnCancelClick(sender, e);
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                // Mostramos el mensaje específico del servidor (ej. "Código inválido")
                MessageBox.Show(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error changing password: " + ex.Message);
            }
            finally
            {
                btnChangePassword.IsEnabled = true;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            // Reseteamos la vista por si vuelve a entrar
            pnlEmailStep.Visibility = Visibility.Visible;
            pnlPasswordStep.Visibility = Visibility.Collapsed;
            txtEmail.IsEnabled = true;
            txtEmail.Clear();
            txtCode.Clear();
            pwdNewPassword.Clear();
            pwdConfirmPassword.Clear();
            txtNewPasswordVisible.Clear();
            txtConfirmPasswordVisible.Clear();

            this.Visibility = Visibility.Collapsed;
        }

        private void ChkShowPasswords_Checked(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = true;
            TogglePasswordVisibility(pwdNewPassword, txtNewPasswordVisible);
            TogglePasswordVisibility(pwdConfirmPassword, txtConfirmPasswordVisible);
        }

        private void ChkShowPasswords_Unchecked(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = false;
            TogglePasswordVisibility(pwdNewPassword, txtNewPasswordVisible);
            TogglePasswordVisibility(pwdConfirmPassword, txtConfirmPasswordVisible);
        }

        private void TogglePasswordVisibility(PasswordBox passwordBox, TextBox textBox)
        {
            if (isPasswordVisible)
            {
                textBox.Text = passwordBox.Password;
                passwordBox.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
            }
            else
            {
                passwordBox.Password = textBox.Text;
                textBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
            }
        }

        private void PwdNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible) txtNewPasswordVisible.Text = pwdNewPassword.Password;
        }

        private void TxtNewPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPasswordVisible) pwdNewPassword.Password = txtNewPasswordVisible.Text;
        }

        private void PwdConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible) txtConfirmPasswordVisible.Text = pwdConfirmPassword.Password;
        }

        private void TxtConfirmPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPasswordVisible) pwdConfirmPassword.Password = txtConfirmPasswordVisible.Text;
        }
    }
}