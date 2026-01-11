using GuessWhoClient.Presentation.ViewModels.Friends;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Friends
{
    public partial class FriendView : UserControl
    {
        public FriendView()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                if (DataContext is FriendViewModel vm)
                {
                    if (vm.LoadFriendsCommand.CanExecute(null))
                        vm.LoadFriendsCommand.Execute(null);
                }
            };
        }
    }
}