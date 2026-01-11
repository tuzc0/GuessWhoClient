using GuessWhoClient.Presentation.ViewModels.Friends;
using System;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Friends
{
    public partial class FriendView : UserControl
    {
        public FriendView()
        {
            InitializeComponent();
        }

        public FriendView(FriendViewModel viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();
            this.DataContext = viewModel;

            this.Loaded += (s, e) =>
            {
                if (viewModel.LoadFriendsCommand.CanExecute(null))
                {
                    viewModel.LoadFriendsCommand.Execute(null);
                }
            };
        }
    }
}