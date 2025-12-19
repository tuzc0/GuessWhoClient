using GuessWhoClient.Dtos;
using GuessWhoClient.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GuessWhoClient
{
    public partial class ChooseCharacterWindow : UserControl
    {
        private readonly ChooseCharacterViewModel viewModel;

        private Border lastSelectedBorder;
        private bool hasConfirmedSelection;

        public ChooseCharacterWindow(ChooseCharacterViewModel viewModel)
        {
            this.viewModel = viewModel
                ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();
            DataContext = this.viewModel;

            btnCancel.Visibility = Visibility.Collapsed;
        }

        private void CharacterCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (hasConfirmedSelection || viewModel.IsSelectionLocked)
            {
                return;
            }

            if (sender is Border border && border.DataContext is CharacterCard character)
            {
                if (lastSelectedBorder != null)
                {
                    lastSelectedBorder.Opacity = 1.0;
                }

                border.Opacity = 0.5;
                lastSelectedBorder = border;

                viewModel.SelectedCharacter = character;
            }
        }

        private async void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedCharacter == null)
            {
                MessageBox.Show(
                    "Debes seleccionar un personaje antes de confirmar.",
                    "Selección requerida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                "¿Estás seguro de que quieres elegir este personaje como tu personaje secreto?\n" +
                "Después de confirmar ya no podrás cambiarlo.",
                "Confirmar selección",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }

            var result = await viewModel.ConfirmSelectionAsync();

            if (!result.Success)
            {
                MessageBox.Show(
                    result.ErrorMessage,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            hasConfirmedSelection = true;

            btnConfirm.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Visible;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.IsSelectionLocked)
            {
                MessageBox.Show(
                    "La partida está a punto de comenzar, ya no puedes cambiar tu personaje.",
                    "Cambio no permitido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            hasConfirmedSelection = false;

            if (lastSelectedBorder != null)
            {
                lastSelectedBorder.Opacity = 1.0;
                lastSelectedBorder = null;
            }

            viewModel.SelectedCharacter = null;

            btnCancel.Visibility = Visibility.Collapsed;
            btnConfirm.Visibility = Visibility.Visible;
        }
    }
}
