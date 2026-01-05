using GuessWhoClient.Presentation.ViewModels.Friends;
using System;
using System.Windows;

namespace WPFGuessWhoClient.Presentation.ViewModels
{
    public partial class FriendManagerWindow : Window
    {
        public FriendManagerWindow(FriendViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            this.Loaded += async (s, e) =>
            {
                if (viewModel.LoadFriendsCommand.CanExecute(null))
                    await viewModel.LoadFriendsAsync();

                if (viewModel.LoadPendingRequestsCommand.CanExecute(null))
                    await viewModel.LoadPendingRequestsAsync();
            };
        }
    }
}