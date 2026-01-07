using GuessWhoClient.Presentation.ViewModels.Error;
using System;
using System.Windows;

namespace GuessWhoClient.Presentation.Views.Windows
{
    public partial class GameMessageWindow : Window
    {
       public GameMessageWindow()
        {
            InitializeComponent();
        }

        public GameMessageWindow(GameMessageViewModel viewModel) : this()
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close ();
        }
    }
}
