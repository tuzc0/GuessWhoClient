using GuessWhoClient.Application.Services.Leaderboard; // Ajusta según tu namespace real
using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Interfaces;
using GuessWhoClient.Presentation.ViewModels.Base;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using GuessWhoClient.Session;
using GuessWhoContracts.Dtos.Dto;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GuessWhoClient.Presentation.ViewModels.Leaderboard
{
    public sealed class LeaderboardViewModel : ViewModelBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LeaderboardViewModel));

        private const string LOG_CTX_LOAD = "LeaderboardViewModel.LoadLeaderboard.Unexpected";

        private const string KEY_UI_ERROR_TITLE = "LeaderboardErrorTitle";
        private const string KEY_GENERIC_ERROR = "UiGenericError";

        private readonly ILeaderboardAppService leaderboardAppService;
        private readonly IAlertService alertService;
        private readonly IUiFaultMapper leaderboardFaultMapper;
        private readonly ILocalizationService localizationService;
        private readonly SessionContext sessionContext;

        private ObservableCollection<LeaderboardPlayerDto> players;
        private LeaderboardPlayerDto currentUserStats;
        private int topN = 10;

        public LeaderboardViewModel(
            ILeaderboardAppService leaderboardAppService,
            IAlertService alertService,
            IUiFaultMapper leaderboardFaultMapper,
            ILocalizationService localizationService,
            SessionContext sessionContext)
        {
            this.leaderboardAppService = leaderboardAppService ?? throw new ArgumentNullException(nameof(leaderboardAppService));
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.leaderboardFaultMapper = leaderboardFaultMapper ?? throw new ArgumentNullException(nameof(leaderboardFaultMapper));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));

            Players = new ObservableCollection<LeaderboardPlayerDto>();
            LoadLeaderboardCommand = new AsyncRelayCommand(LoadLeaderboardAsync, CanExecuteCommands);
        }

        public ObservableCollection<LeaderboardPlayerDto> Players
        {
            get => players;
            set => SetProperty(ref players, value);
        }

        public LeaderboardPlayerDto CurrentUserStats
        {
            get => currentUserStats;
            set => SetProperty(ref currentUserStats, value);
        }

        public int TopN
        {
            get => topN;
            set => SetProperty(ref topN, value);
        }

        public AsyncRelayCommand LoadLeaderboardCommand { get; }

        protected override void OnIsBusyChanged(string propertyName)
        {
            LoadLeaderboardCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCommands() => !IsBusy;

        private async Task LoadLeaderboardAsync()
        {
            IsBusy = true;
            try
            {
                var request = new GetLeaderboardRequest
                {
                    TopN = this.TopN,
                    RequestingUserId = sessionContext.UserId
                };

                var result = await leaderboardAppService.GetGlobalLeaderboardAsync(request);

                if (!result.IsSuccess)
                {
                    ShowLeaderboardError(result.FaultCode);
                    return;
                }

                if (result.Value != null)
                {
                    var playerList = result.Value.Players != null
                                     ? new System.Collections.Generic.List<LeaderboardPlayerDto>(result.Value.Players)
                                     : new System.Collections.Generic.List<LeaderboardPlayerDto>();

                    Players = new ObservableCollection<LeaderboardPlayerDto>(playerList);
                    CurrentUserStats = result.Value.CurrentUserStats;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LOG_CTX_LOAD, ex);
                ShowUiError(KEY_GENERIC_ERROR);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowUiError(string messageKey)
        {
            alertService.Error(localizationService.Get(messageKey), localizationService.Get(KEY_UI_ERROR_TITLE));
        }

        private void ShowLeaderboardError(string faultCode)
        {
            var mapping = leaderboardFaultMapper.Map(faultCode);
            string key = mapping.IsMapped ? mapping.UiKey : KEY_GENERIC_ERROR;
            alertService.Error(localizationService.Get(key), localizationService.Get(KEY_UI_ERROR_TITLE));
        }
    }
}