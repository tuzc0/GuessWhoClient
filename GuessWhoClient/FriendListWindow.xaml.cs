using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using System.Windows;
using GuessWhoClient.Alerts;
using GuessWhoClient.Globalization;
using GuessWhoClient.FriendServiceRef;
using GuessWhoClient.Dtos;
using log4net;
using System.Windows.Controls;
using System.Collections.Generic;

using ServiceFault = GuessWhoClient.FriendServiceRef.ServiceFault;

namespace WPFGuessWhoClient
{
    public partial class FriendsListWindow : Window
    {
        private readonly FriendServiceClient _service;
        private readonly long _currentAccountId;

        private readonly IAlertService alertService = new MessageBoxAlertService();
        private readonly ILocalizationService localizationService = new LocalizationService();

        public FriendsListWindow(long accountId)
        {
            InitializeComponent();
            _currentAccountId = accountId;

            if (_currentAccountId <= 0)
            {
                alertService.Error(localizationService.Get("UiInvalidAccountId"));
                this.Close();
                return;
            }

            try
            {
                _service = new FriendServiceClient();
            }
            catch (Exception ex)
            {
                alertService.Error(localizationService.Get("UiCommsGeneric") + Environment.NewLine + ex.Message);
                this.Close();
                return;
            }

            this.Loaded += async (s, e) =>
            {
                await LoadFriendsList();
            };
        }

        private async Task LoadFriendsList()
        {
            FriendServiceClient client = _service;

            try
            {
                var request = new GetFriendsRequest { AccountId = _currentAccountId.ToString() };
                var response = await client.GetFriendsAsync(request);

                if (response.Friends != null)
                {
                    DgFriends.ItemsSource = response.Friends;
                }
                else
                {
                    DgFriends.ItemsSource = null;
                }
            }
            catch (SecurityNegotiationException ex)
            {
                string message =
                    localizationService.Get("UiSecurityNegotiationFailed") +
                    Environment.NewLine +
                    Environment.NewLine +
                    ex.Message;

                alertService.Error(message);
                client.Abort();
            }
            catch (FaultException<ServiceFault> ex)
            {
                string faultKey = $"Fault{ex.Detail.Code}";
                string localizedText = localizationService.LocalOrFallback(
                    faultKey,
                    ex.Detail.Message,
                    "FaultUnexpected");

                alertService.Error(localizedText);
                DgFriends.ItemsSource = null;
                client.Abort();
            }
            catch (TimeoutException ex)
            {
                alertService.Error(localizationService.Get("FaultDatabaseTimeout"));
                client.Abort();
            }
            catch (CommunicationException ex)
            {
                string message =
                    localizationService.Get("UiCommsGeneric") +
                    Environment.NewLine +
                    Environment.NewLine +
                    ex.Message;

                alertService.Error(message);
                client.Abort();
            }
            catch (Exception ex)
            {
                string message =
                    localizationService.Get("FaultUnexpected") +
                    Environment.NewLine +
                    Environment.NewLine +
                    ex.Message;

                alertService.Error(message);
                client.Abort();
            }
        }

        private async void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            await CloseFriendServiceClientSafelyAsync(_service);
            this.Close();
        }

        private static async Task CloseFriendServiceClientSafelyAsync(FriendServiceClient client)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    await Task.Run(() => client.Close());
                }
            }
            catch
            {
                client.Abort();
            }
        }
    }
}