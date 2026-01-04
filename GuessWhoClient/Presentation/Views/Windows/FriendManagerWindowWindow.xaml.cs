using GuessWhoClient.Presentation.ViewModels.Friends;
using System;
using System.Windows;
using ClassLibraryGuessWho.Properties.Localization;


namespace WPFGuessWhoClient
{
    public partial class FriendManagerWindow : Window
    {
        public FriendManagerWindow(FriendViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            this.Loaded += (s, e) =>
            {
                if (viewModel.LoadFriendsCommand != null && viewModel.LoadFriendsCommand.CanExecute(null))
                {
                    viewModel.LoadFriendsCommand.Execute(null);
                }
            };
        }
    }
}