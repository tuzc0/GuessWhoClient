using System;
using System.ServiceModel;
using System.Windows;
using GuessWhoClient.FriendServiceRef;
using System.Linq; 
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WPFGuessWhoClient
{
    public partial class FriendManagerWindow : Window
    {
        private readonly FriendServiceClient _service;
        private readonly long _currentAccountId;

        public FriendManagerWindow(long accountId)
        {
            InitializeComponent();
            _currentAccountId = accountId;

            try
            {
                _service = new FriendServiceClient();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to Friend Service. Check your connection.\n" + ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            this.Loaded += async (s, e) =>
            {
                await LoadFriendsData();
            };
        }

        private async Task LoadFriendsData()
        {
            await LoadFriendsList();
            await RefreshPendingRequests();
        }

        private async Task LoadFriendsList()
        {
            try
            {
                var request = new GetFriendsRequest { AccountId = _currentAccountId.ToString() };
                var response = await _service.GetFriendsAsync(request);

                if (response.Friends != null)
                {
                    DgFriends.ItemsSource = response.Friends;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading friends list: " + ex.Message, "Friend List Error");
            }
        }

        private async Task RefreshPendingRequests()
        {
            try
            {
                LblStatus.Text = "Refreshing requests...";

                var request = new GetPendingFriendRequestsRequest { AccountId = _currentAccountId.ToString() };
                var response = await _service.GetPendingRequestsAsync(request);

                if (response.Requests != null)
                {
                    var receivedRequests = response.Requests
                        .Where(r => r.AddresseeUserId == _currentAccountId)
                        .ToList();

                    DgReceivedRequests.ItemsSource = receivedRequests;

                    int receivedCount = receivedRequests.Count;

                    LblStatus.Text = $"{receivedCount} received requests.";
                }
                else
                {
                    DgReceivedRequests.ItemsSource = null;
                    LblStatus.Text = "No pending requests.";
                }
            }
            catch (FaultException<ServiceFault> fault)
            {
                MessageBox.Show($"Server Error:\n{fault.Detail.Message}", "Request Load Failed");
                LblStatus.Text = "Error loading requests.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error:\n{ex.Message}", "Connection Error");
                LblStatus.Text = "Connection error.";
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string query = TxtSearch.Text.Trim();

            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Please enter a name to search.", "Validation");
                return;
            }

            try
            {
                LblStatus.Text = "Searching...";

                var request = new SearchProfileRequest
                {
                    DisplayName = query
                };

                var response = _service.SearchProfiles(request);

                if (response.Profiles != null)
                {
                    DgProfiles.ItemsSource = response.Profiles;
                    LblStatus.Text = $"{response.Profiles.Length} profiles found.";
                }
                else
                {
                    DgProfiles.ItemsSource = null;
                    LblStatus.Text = "No profiles found.";
                }
            }
            catch (FaultException<ServiceFault> fault)
            {
                MessageBox.Show($"Server Logic Error:\n{fault.Detail.Message}", "Search Failed");
                LblStatus.Text = "Error.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error:\n{ex.Message}", "Error");
                LblStatus.Text = "Error.";
            }
        }

        private async void BtnSendRequest_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = DgProfiles.SelectedItem as UserProfileSearchResult;

            if (selectedProfile == null)
            {
                MessageBox.Show("Select a user from the list to add.", "Hint");
                return;
            }

            try
            {
                var request = new SendFriendRequestRequest
                {
                    FromAccountId = _currentAccountId,
                    ToUserId = selectedProfile.UserId
                };

                var response = await _service.SendFriendRequestAsync(request);

                if (response.AutoAccepted)
                {
                    MessageBox.Show($"You are now friends with {selectedProfile.DisplayName}!", "It's a Match!");
                }
                else if (response.Success)
                {
                    MessageBox.Show($"Friend request sent to {selectedProfile.DisplayName}.", "Success");
                }
                else
                {
                    MessageBox.Show("Request could not be sent. A pending request might already exist.", "Info");
                }

                await LoadFriendsData();
            }
            catch (FaultException<ServiceFault> fault)
            {
                MessageBox.Show($"Server Error:\n{fault.Detail.Message}", "Request Failed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Error");
            }
        }

        private async void BtnRefreshRequests_Click(object sender, RoutedEventArgs e)
        {
            await RefreshPendingRequests();
        }

        private async void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            await ProcessRequest("Accept", DgReceivedRequests);
        }

        private async void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            await ProcessRequest("Reject", DgReceivedRequests);
        }


        private async Task ProcessRequest(string action, System.Windows.Controls.DataGrid sourceDataGrid)
        {
            var selectedItem = sourceDataGrid.SelectedItem;

            if (selectedItem == null)
            {
                MessageBox.Show($"Select a request from the {sourceDataGrid.Name} list first.", "Validation");
                return;
            }

            long requestId = 0;

            try
            {
                requestId = ((FriendRequest)selectedItem).FriendRequestId;
            }
            catch
            {
                MessageBox.Show("Invalid item selection. Please ensure you selected a row.", "Error");
                return;
            }

            try
            {
                var request = new AcceptFriendRequestRequest
                {
                    AccountId = _currentAccountId.ToString(),
                    FriendRequestId = requestId.ToString()
                };

                BasicResponse response = null;

                switch (action)
                {
                    case "Accept":
                        response = await _service.AcceptFriendRequestAsync(request);
                        break;
                    case "Reject":
                        response = await _service.RejectFriendRequestAsync(request);
                        break;
                }

                if (response != null && response.Success)
                {
                    MessageBox.Show($"Request {action}ed successfully.", "Done");
                }

                await LoadFriendsData();

            }
            catch (FaultException<ServiceFault> fault)
            {
                MessageBox.Show($"Server Logic Error:\n{fault.Detail.Message}", $"{action} Failed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Error");
            }
        }
    }
}