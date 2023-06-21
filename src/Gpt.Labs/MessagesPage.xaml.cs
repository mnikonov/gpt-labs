using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Base;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System;
using Gpt.Labs.Helpers;

namespace Gpt.Labs
{
    public sealed partial class MessagesPage : StatePage
    {
        #region Fields

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(NotifyTaskCompletion<MessagesListViewModel>),
            typeof(MessagesPage),
            new PropertyMetadata(null, null));  

        #endregion

        #region Constructors

        public MessagesPage()
        {
            this.ViewModel = new NotifyTaskCompletion<MessagesListViewModel>();

            this.InitializeComponent();
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

        public override void LoadState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode)
        {
            base.LoadState(destinationPageType, parameters, state, mode);

            ViewModel.Function = async token =>
            {
                var viewModel = new MessagesListViewModel(() => this.RootPage);

                await viewModel.LoadStateAsync(destinationPageType, parameters, state, mode);

                var chatPage = this.GetParent<ChatsPage>();
                
                if (chatPage != null && chatPage.ViewModel.IsCompleted)
                {
                    chatPage.ViewModel.Result.SetSelection(viewModel.ChatId);
                }

                return viewModel;
            };
                        
            ViewModel.Start();
        }

        public override void SaveState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode)
        {
            ViewModel.Result?.SaveState(destinationPageType, parameters, state, mode);
            base.SaveState(destinationPageType, parameters, state, mode);
        }

        #endregion
    }
}
