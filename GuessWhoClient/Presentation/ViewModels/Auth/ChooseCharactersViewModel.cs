using GuessWhoClient.Assets;
using GuessWhoClient.Dtos;
using GuessWhoClient.Interfaces;
using GuessWhoCore.Contracts.Faults;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading.Tasks;
using WPFGuessWhoClient.Application.Results;

namespace GuessWhoClient.ViewModels
{
    public sealed class ChooseCharacterViewModel : INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChooseCharacterViewModel));

        private readonly IMatchSessionController matchSessionController;

        public ObservableCollection<CharacterCard> Characters { get; }
            = new ObservableCollection<CharacterCard>();

        private CharacterCard selectedCharacter;
        public CharacterCard SelectedCharacter
        {
            get { return selectedCharacter; }
            set
            {
                if (selectedCharacter != value)
                {
                    selectedCharacter = value;
                    OnPropertyChanged(nameof(SelectedCharacter));
                }
            }
        }

        private bool isSelectionLocked;
        public bool IsSelectionLocked
        {
            get { return isSelectionLocked; }
            set
            {
                if (isSelectionLocked != value)
                {
                    isSelectionLocked = value;
                    OnPropertyChanged(nameof(IsSelectionLocked));
                }
            }
        }

        public ChooseCharacterViewModel(IMatchSessionController matchSessionController)
        {
            this.matchSessionController = matchSessionController
                ?? throw new ArgumentNullException(nameof(matchSessionController));

            _ = LoadCharactersAsync();
        }

        private const int DEFAULT_DECK_SIZE = 24;

        private async Task LoadCharactersAsync()
        {
            var deckIds = await matchSessionController.GetMatchDeckAsync(
                matchSessionController.MatchId,
                DEFAULT_DECK_SIZE);

            Characters.Clear();

            foreach (var id in deckIds)
            {
                Characters.Add(new CharacterCard
                {
                    Id = id,
                    ImagePath = CharacterAssets.GetCharacterPathById(id)
                });
            }
        }


        public async Task<OperationResult> ConfirmSelectionAsync()
        {
            if (SelectedCharacter == null)
            {
                return OperationResult.Fail("Debes seleccionar un personaje.");
            }

            try
            {
                await matchSessionController.ChooseSecretCharacterAsync(SelectedCharacter.Id);
                return OperationResult.Ok();
            }
            catch (FaultException<ServiceFault> ex)
            {
                Logger.Error("ConfirmSelectionAsync: service fault.", ex);

                string message = ex.Detail != null && !string.IsNullOrWhiteSpace(ex.Detail.MessageKey)
                    ? ex.Detail.MessageKey
                    : "Ocurrió un error en el servidor al elegir tu personaje secreto.";

                return OperationResult.Fail(message);
            }
            catch (FaultException ex)
            {
                Logger.Error("ConfirmSelectionAsync: fault exception.", ex);
                return OperationResult.Fail(ex.Message);
            }
            catch (TimeoutException ex)
            {
                Logger.Error("ConfirmSelectionAsync: timeout.", ex);
                return OperationResult.Fail("La solicitud para elegir tu personaje secreto excedió el tiempo de espera.");
            }
            catch (CommunicationException ex)
            {
                Logger.Error("ConfirmSelectionAsync: communication error.", ex);
                return OperationResult.Fail("No fue posible comunicarse con el servidor para elegir tu personaje secreto.");
            }
            catch (Exception ex)
            {
                Logger.Error("ConfirmSelectionAsync: unexpected error.", ex);
                return OperationResult.Fail("Ocurrió un error inesperado al elegir tu personaje secreto.");
            }
        }

        public void LockSelection()
        {
            IsSelectionLocked = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
