using GuessWhoClient.Dtos;
using GuessWhoClient.Interfaces;
using GuessWhoClient.MatchServiceRef;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace GuessWhoClient.ViewModels
{
    public sealed class LobbyViewModel : INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LobbyViewModel));

        private readonly IMatchSessionController matchSessionController;

        public event PropertyChangedEventHandler PropertyChanged;

        public long MatchId => matchSessionController.MatchId;

        public long CurrentUserId => matchSessionController.CurrentUserId;

        public string MatchCode { get; }

        public ObservableCollection<ClientLobbyPlayerDto> LobbyPlayers { get; }

        public bool IsCurrentUserHost =>
            LobbyPlayers.FirstOrDefault(p => p.UserId == CurrentUserId)?.IsHost ?? false;

        public LobbyViewModel(GamePlayParameters gamePlayParameters, IMatchSessionController sessionController)
        {
            matchSessionController = sessionController
                ?? throw new ArgumentNullException(nameof(sessionController));

            if (gamePlayParameters == null)
            {
                throw new ArgumentNullException(nameof(gamePlayParameters));
            }

            MatchCode = gamePlayParameters.MatchCode;

            LobbyPlayers = new ObservableCollection<ClientLobbyPlayerDto>(
                gamePlayParameters.Players ?? Array.Empty<ClientLobbyPlayerDto>());

            matchSessionController.PlayerJoined += OnPlayerJoined;
            matchSessionController.PlayerLeft += OnPlayerLeft;
            matchSessionController.ReadyChanged += OnReadyChanged;
            matchSessionController.GameStarted += OnGameStarted;
        }

        private void OnPlayerJoined(ClientLobbyPlayerDto player)
        {
            if (player == null)
            {
                return;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                LobbyPlayers.Add(player);

                OnPropertyChanged(nameof(IsCurrentUserHost));
            });
        }

        private void OnPlayerLeft(ClientLobbyPlayerDto player)
        {
            if (player == null)
            {
                return;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                var existing = LobbyPlayers.FirstOrDefault(p => p.UserId == player.UserId);
                if (existing != null)
                {
                    LobbyPlayers.Remove(existing);
                    OnPropertyChanged(nameof(IsCurrentUserHost));
                }
                else
                {
                    Logger.WarnFormat("OnPlayerLeft: player with UserId={0} not found in LobbyPlayers.", player.UserId);
                }
            });
        }

        private void OnReadyChanged(ClientLobbyPlayerDto player)
        {
            if (player == null)
            {
                return;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                var existing = LobbyPlayers.FirstOrDefault(p => p.UserId == player.UserId);

                if (existing != null)
                {
                    existing.IsReady = player.IsReady;
                    // Si algún día tienes bindings que dependan del “estado agregado”
                    // también podrías disparar OnPropertyChanged aquí.
                }
                else
                {
                    Logger.WarnFormat(
                        "OnReadyChanged: player with UserId={0} not found in LobbyPlayers.",
                        player.UserId);
                }
            });
        }

        private void OnGameStarted()
        {
            // Aquí sigues sin tocar UI directamente.
            // Si luego necesitas que la ventana reaccione, puedes:
            // - Exponer un evento público (e.g. MatchStarted),
            // - o una propiedad bool IsGameStarted con OnPropertyChanged.
        }

        public async Task<OperationResult> LeaveAsync()
        {
            try
            {
                await matchSessionController.LeaveMatchAsync();
                return OperationResult.Ok();
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("LeaveAsync: service fault.", ex);

                string message = ex.Detail != null && !string.IsNullOrWhiteSpace(ex.Detail.Message)
                    ? ex.Detail.Message
                    : "Ocurrió un error en el servidor al salir del lobby.";

                return OperationResult.Fail(message);
            }
            catch (FaultException ex)
            {
                Logger.Error("LeaveAsync: fault exception.", ex);
                return OperationResult.Fail(ex.Message);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("LeaveAsync: timeout.", ex);
                return OperationResult.Fail("La solicitud para salir del lobby excedió el tiempo de espera.");
            }
            catch (CommunicationException ex)
            {
                Logger.Error("LeaveAsync: communication error.", ex);
                return OperationResult.Fail("No fue posible comunicarse con el servidor para salir del lobby.");
            }
            catch (Exception ex)
            {
                Logger.Error("LeaveAsync: unexpected error.", ex);
                return OperationResult.Fail("Ocurrió un error inesperado al salir del lobby.");
            }
        }

        public async Task<OperationResult> SetReadyAsync()
        {
            try
            {
                await matchSessionController.SetPlayerReadyAsync();
                return OperationResult.Ok();
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("SetReadyAsync: service fault.", ex);

                string message = ex.Detail != null && !string.IsNullOrWhiteSpace(ex.Detail.Message)
                    ? ex.Detail.Message
                    : "Ocurrió un error en el servidor al marcar tu estado como listo.";

                return OperationResult.Fail(message);
            }
            catch (FaultException ex)
            {
                Logger.Error("SetReadyAsync: fault exception.", ex);
                return OperationResult.Fail(ex.Message);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("SetReadyAsync: timeout.", ex);
                return OperationResult.Fail("La solicitud para marcar tu estado como listo excedió el tiempo de espera.");
            }
            catch (CommunicationException ex)
            {
                Logger.Error("SetReadyAsync: communication error.", ex);
                return OperationResult.Fail("No fue posible comunicarse con el servidor para marcar tu estado como listo.");
            }
            catch (Exception ex)
            {
                Logger.Error("SetReadyAsync: unexpected error.", ex);
                return OperationResult.Fail("Ocurrió un error inesperado al marcar tu estado como listo.");
            }
        }

        public async Task<OperationResult> StartMatchAsync()
        {
            try
            {
                await matchSessionController.StartMatchAsync();
                return OperationResult.Ok();
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("StartMatchAsync: service fault.", ex);

                string message = ex.Detail != null && !string.IsNullOrWhiteSpace(ex.Detail.Message)
                    ? ex.Detail.Message
                    : "Ocurrió un error en el servidor al iniciar la partida.";

                return OperationResult.Fail(message);
            }
            catch (FaultException ex)
            {
                Logger.Error("StartMatchAsync: fault exception.", ex);
                return OperationResult.Fail(ex.Message);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("StartMatchAsync: timeout.", ex);
                return OperationResult.Fail("La solicitud para iniciar la partida excedió el tiempo de espera.");
            }
            catch (CommunicationException ex)
            {
                Logger.Error("StartMatchAsync: communication error.", ex);
                return OperationResult.Fail("No fue posible comunicarse con el servidor para iniciar la partida.");
            }
            catch (Exception ex)
            {
                Logger.Error("StartMatchAsync: unexpected error.", ex);
                return OperationResult.Fail("Ocurrió un error inesperado al iniciar la partida.");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

