using GuessWhoClient.Presentation.ViewModels.Profile;
using log4net;
using System;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Views.Profile
{
    public partial class UpdateProfileView : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateProfileView));
        private const string LOG_CTX_LOADED = "UpdateProfileView.Loaded";

        private readonly UpdateProfileViewModel viewModel;

        public UpdateProfileView(UpdateProfileViewModel viewModel)
        {
            InitializeComponent();

            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = this.viewModel;

            Loaded += UpdateProfileView_Loaded;
        }

        private async void UpdateProfileView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Loaded -= UpdateProfileView_Loaded;
                await viewModel.LoadProfileAsync();
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(LOG_CTX_LOADED, ex);
            }
        }
    }
}