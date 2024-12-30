using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Base;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gpt.Labs
{
    public sealed partial class SingleChatPage : Page
    {
        #region Fields

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(NotifyTaskCompletion<MessagesListViewModel>),
            typeof(MessagesPage),
            new PropertyMetadata(null, null));

        #endregion

        #region Constructors

        public SingleChatPage()
        {
            ViewModel = new NotifyTaskCompletion<MessagesListViewModel>();

            InitializeComponent();
        }

        #endregion

        #region Properties

        public NotifyTaskCompletion<MessagesListViewModel> ViewModel
        {
            get => (NotifyTaskCompletion<MessagesListViewModel>)GetValue(ViewModelProperty);

            private set => SetValue(ViewModelProperty, value);
        }

        #endregion

        #region Public Methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Function = async token =>
            {
                var viewModel = new MessagesListViewModel(() => Frame.GetParent<BasePage>());

                await viewModel.LoadStateAsync(e.SourcePageType, Query.Parse(e.Parameter), null, e.NavigationMode);

                return viewModel;
            };

            ViewModel.Start();

            base.OnNavigatedTo(e);
        }

        #endregion
    }
}
