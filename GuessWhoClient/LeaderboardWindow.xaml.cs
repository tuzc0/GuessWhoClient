using GuessWhoClient.LeaderboardServiceRef;
using GuessWhoClient.Session; // Necesario para acceder al ID del usuario
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace GuessWhoClient
{
    public partial class LeaderboardWindow : Window
    {
        public ObservableCollection<LeaderboardPlayerDto> TopPlayers { get; set; }

        public LeaderboardPlayerDto MyStats { get; set; }

        public LeaderboardWindow()
        {
            InitializeComponent();
            TopPlayers = new ObservableCollection<LeaderboardPlayerDto>();
            DataContext = this;

            Loaded += LeaderboardWindow_Loaded;
        }

        private LeaderboardServiceClient CreateLeaderboardClient()
        {
            return new LeaderboardServiceClient();
        }

        private async void LeaderboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsEnabled = false;

                int userId = (int)SessionContext.Current.UserId;

                var request = new GetLeaderboardRequest
                {
                    TopN = 10,
                    RequestingUserId = userId
                };

                using (var client = CreateLeaderboardClient())
                {
                    var response = await client.GetGlobalLeaderboardAsync(request);

                    TopPlayers.Clear();

                    if (response?.Players != null)
                    {
                        foreach (var player in response.Players)
                        {
                            TopPlayers.Add(player);
                        }
                    }

                    MyStats = response?.CurrentUserStats;

                    if (MyStats != null)
                    {
                        DataContext = null;
                        DataContext = this;
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                MessageBox.Show(ex.Detail.Message, "Error al obtener marcadores");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error inesperado");
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}