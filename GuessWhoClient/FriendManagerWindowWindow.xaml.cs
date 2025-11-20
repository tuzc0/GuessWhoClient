using System;
using System.ServiceModel;
using System.Windows;
using GuessWhoClient.FriendServiceRef; 

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

        private void BtnSendRequest_Click(object sender, RoutedEventArgs e)
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

                var response = _service.SendFriendRequest(request);

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
                    MessageBox.Show("Request could not be sent. It might already exist.", "Info");
                }
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

        private void BtnRefreshRequests_Click(object sender, RoutedEventArgs e)
        {


            MessageBox.Show("The current Server implementation does not include a method to fetch Pending Requests.\n\n" +
                            "Please implement 'GetFriendRequests' in the WCF Service to populate this list.",
                            "Server Limitation", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            ProcessRequest("Accept");
        }

        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            ProcessRequest("Reject");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ProcessRequest("Cancel");
        }

        private void ProcessRequest(string action)
        {

            var selectedItem = DgRequests.SelectedItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Select a request from the Pending list first.", "Validation");
                return;
            }

            long requestId = 0;

            try
            {
                requestId = (long)((dynamic)selectedItem).FriendRequestId;
            }
            catch
            {
                MessageBox.Show("Invalid item selection.", "Error");
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
                        response = _service.AcceptFriendRequest(request);
                        break;
                    case "Reject":
                        response = _service.RejectFriendRequest(request);
                        break;
                    case "Cancel":
                        response = _service.CancelFriendRequest(request);
                        break;
                }

                if (response != null && response.Success)
                {
                    MessageBox.Show($"Request {action}ed successfully.", "Done");
                }
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