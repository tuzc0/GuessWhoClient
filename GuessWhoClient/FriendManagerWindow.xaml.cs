using GuessWhoClient.FriendServiceRef;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class FriendManagerWindow : Window
    {
        private readonly FriendServiceClient _serviceProxy;
        private readonly long _currentAccountId;

        // ==========================
        // CONSTRUCTOR SIN PARÁMETROS (para WPF StartupUri)
        // ==========================
        public FriendManagerWindow()
        {
            InitializeComponent();

            // Inicialización por defecto para WPF
            _currentAccountId = 0;
            _serviceProxy = new FriendServiceClient("NetTcpBinding_IFriendService");

            SetStatus("Ready", false);
        }

        // ==========================
        // CONSTRUCTOR CON PARÁMETROS (para inyección de currentAccountId)
        // ==========================
        public FriendManagerWindow(long currentAccountId, string endpointConfigurationName = "NetTcpBinding_IFriendService")
        {
            InitializeComponent();

            _currentAccountId = currentAccountId;
            _serviceProxy = new FriendServiceClient(endpointConfigurationName);

            SetStatus("Ready", false);
        }

        // ==========================
        // SEARCH USER BY USERNAME
        // ==========================
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string displayName = TxtSearch.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(displayName))
            {
                SetStatus("Please enter a username.", true);
                return;
            }

            try
            {
                SetStatus("Searching...", false);

                var req = new SearchProfileRequest
                {
                    DisplayName = displayName
                };

                var resp = _serviceProxy.SearchProfilesAsync(req).Result;

                if (resp?.Profiles != null)
                {
                    DgProfiles.ItemsSource = resp.Profiles;
                    SetStatus($"{resp.Profiles.Length} profile(s) found.", false);
                }
                else
                {
                    DgProfiles.ItemsSource = null;
                    SetStatus("No profiles found.", true);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", true);
            }
        }

        // ==========================
        // SEND FRIEND REQUEST
        // ==========================
        private void BtnSendRequest_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgProfiles.SelectedItem as UserProfileSearchResult;
            if (selected == null)
            {
                MessageBox.Show("Select a profile first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                SetStatus("Sending request...", false);

                var req = new SendFriendRequestRequest
                {
                    FromAccountId = _currentAccountId,
                    ToUserId = selected.UserId
                };

                var resp = _serviceProxy.SendFriendRequestAsync(req).Result;

                if (resp == null)
                {
                    SetStatus("Service returned no response.", true);
                    return;
                }

                if (resp.Success)
                {
                    if (resp.AutoAccepted)
                        MessageBox.Show("Friend request auto-accepted!", "Info");
                    else
                        MessageBox.Show($"Friend request sent! ID = {resp.FriendRequestId}", "Info");

                    SetStatus("Request completed.", false);
                }
                else
                {
                    SetStatus("Request failed.", true);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", true);
            }
        }

        // ==========================
        // PENDING REQUESTS (NOT IMPLEMENTED)
        // ==========================
        private void BtnRefreshRequests_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Server does not implement GetPendingRequests(). Add method to server first.",
                "Not Implemented",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // ==========================
        // ACCEPT REQUEST
        // ==========================
        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            string requestId = ExtractFriendRequestId(DgRequests.SelectedItem);
            if (requestId == null)
            {
                MessageBox.Show("Select a valid request.", "Error");
                return;
            }

            var req = new AcceptFriendRequestRequest
            {
                AccountId = _currentAccountId.ToString(),
                FriendRequestId = requestId
            };

            try
            {
                var resp = _serviceProxy.AcceptFriendRequestAsync(req).Result;

                if (resp?.Success == true)
                    MessageBox.Show("Request accepted!", "Info");
                else
                    MessageBox.Show("Failed to accept request.", "Error");
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", true);
            }
        }

        // ==========================
        // REJECT REQUEST
        // ==========================
        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            string requestId = ExtractFriendRequestId(DgRequests.SelectedItem);
            if (requestId == null)
            {
                MessageBox.Show("Select a valid request.", "Error");
                return;
            }

            var req = new AcceptFriendRequestRequest
            {
                AccountId = _currentAccountId.ToString(),
                FriendRequestId = requestId
            };

            try
            {
                var resp = _serviceProxy.RejectFriendRequestAsync(req).Result;

                if (resp?.Success == true)
                    MessageBox.Show("Request rejected!", "Info");
                else
                    MessageBox.Show("Failed to reject request.", "Error");
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", true);
            }
        }

        // ==========================
        // CANCEL REQUEST
        // ==========================
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            string requestId = ExtractFriendRequestId(DgRequests.SelectedItem);
            if (requestId == null)
            {
                MessageBox.Show("Select a valid request.", "Error");
                return;
            }

            var req = new AcceptFriendRequestRequest
            {
                AccountId = _currentAccountId.ToString(),
                FriendRequestId = requestId
            };

            try
            {
                var resp = _serviceProxy.CancelFriendRequestAsync(req).Result;

                if (resp?.Success == true)
                    MessageBox.Show("Request cancelled!", "Info");
                else
                    MessageBox.Show("Failed to cancel request.", "Error");
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", true);
            }
        }

        // ==========================
        // HELPERS
        // ==========================
        private void SetStatus(string message, bool isError)
        {
            LblStatus.Text = message;
            LblStatus.Foreground = isError
                ? System.Windows.Media.Brushes.OrangeRed
                : System.Windows.Media.Brushes.LightGreen;
        }

        private string ExtractFriendRequestId(object item)
        {
            if (item == null) return null;

            var type = item.GetType();

            var candidates = new[] { "FriendRequestId", "RequestId", "Id" };

            foreach (var name in candidates)
            {
                var p = type.GetProperty(name);
                if (p != null)
                {
                    var val = p.GetValue(item);
                    if (val != null) return val.ToString();
                }
            }

            return null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            try
            {
                if (_serviceProxy.State == CommunicationState.Faulted)
                    _serviceProxy.Abort();
                else
                    _serviceProxy.Close();
            }
            catch
            {
                _serviceProxy.Abort();
            }
        }
    }
}
