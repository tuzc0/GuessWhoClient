using GuessWhoClient.Assets;
using GuessWhoClient.Dtos; // aquí debería estar CharacterCard
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GuessWhoClient
{
    public partial class ChooseCharacterWindow : UserControl
    {
        public ObservableCollection<CharacterCard> Characters { get; } =
            new ObservableCollection<CharacterCard>();

        public CharacterCard SelectedCharacter { get; private set; }

        public ChooseCharacterWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadCharacters();
        }

        private void LoadCharacters()
        {
            var paths = CharacterAssets.GetAllCharacterPaths();

            for (int index = 0; index < paths.Count; index++)
            {
                Characters.Add(new CharacterCard
                {
                    Id = index + 1,
                    ImagePath = paths[index]
                });
            }
        }

        private void CharacterCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is CharacterCard character)
            {
                SelectedCharacter = character;
                // aquí puedes marcar visualmente la selección si quieres
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCharacter == null)
            {
                MessageBox.Show(
                    "Selecciona un personaje antes de continuar.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // TODO: usar SelectedCharacter (enviar al server, guardar, etc.)
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Como es UserControl, normalmente el dueño (Window) decide qué hacer.
            // Si lo alojas dentro de un Window, podrías cerrar así:
            var parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }
}
