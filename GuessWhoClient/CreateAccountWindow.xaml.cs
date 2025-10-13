using GuessWhoClient.UserServiceRef;
using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace GuessWhoClient //Clase de prueba para el servicio de usuario (falta pulirlo)
{
    public partial class CreateAccountWindow : Window
    {
        private static readonly Regex EmailRegex =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public CreateAccountWindow()
        {
            InitializeComponent();
        }

        private async void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {

            var email = (txtEmail.Text ?? string.Empty).Trim().ToLowerInvariant();
            var displayName = (txtDisplayName.Text ?? string.Empty).Trim();
            var password = txtPassword.Password ?? string.Empty;
            var confirm = txtConfirmPassword.Password ?? string.Empty;

            var error = ValidateForm(email, displayName, password, confirm);
            if (error != null)
            {
                MessageBox.Show(error, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnCreateAccount.IsEnabled = false;

            var client = new UserServiceClient("NetTcp_UserService");

            try
            {
                var request = new RegisterRequest
                {
                    Email = email,
                    Password = password,
                    DisplayName = displayName
                };

                var resp = await client.RegisterUserAsync(request);

                MessageBox.Show(
                    $"Cuenta creada para {resp.Email}\nUserId: {resp.UserId}",
                    "Registro exitoso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await SafeCloseAsync(client);

            }
            catch (SecurityNegotiationException ex)
            {
                client.Abort();
                MessageBox.Show(
                    "Fallo autenticación Windows (Transport). " +
                    "Revisa que el servicio esté arriba, Net.Tcp Port Sharing iniciado y Visual Studio en modo administrador.\n\n" + ex.Message,
                    "Seguridad",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (FaultException fe)
            {
                client.Abort();
                MessageBox.Show(fe.Message, "Servicio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (CommunicationException ex)
            {
                client.Abort();
                MessageBox.Show(
                    "No se pudo comunicar con el servicio. ¿Sigue corriendo en 8095?\n\n" + ex.Message,
                    "Comunicación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                client.Abort();
                MessageBox.Show("Error inesperado:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnCreateAccount.IsEnabled = true;
            }
        }

        private static string ValidateForm(string email, string displayName, string password, string confirm)
        {
            if (string.IsNullOrWhiteSpace(email)) return "El correo es requerido.";
            if (!EmailRegex.IsMatch(email)) return "El formato de correo no es válido.";

            if (string.IsNullOrWhiteSpace(displayName)) return "El nombre visible es requerido.";
            if (displayName.Length > 50) return "El nombre visible es demasiado largo (máx. 50).";

            if (string.IsNullOrWhiteSpace(password)) return "La contraseña es requerida.";
            if (password.Length < 8) return "La contraseña debe tener al menos 8 caracteres.";
            if (!string.Equals(password, confirm)) return "Las contraseñas no coinciden.";

            return null;
        }

        private static async Task SafeCloseAsync(UserServiceClient client)
        {
            try
            {
                if (client.State == CommunicationState.Faulted)
                    client.Abort();
                else
                    await Task.Run(() => client.Close());
            }
            catch
            {
                client.Abort();
            }
        }
    }
}
