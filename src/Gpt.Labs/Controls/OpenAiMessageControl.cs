using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gpt.Labs.Models;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.System;
using Microsoft.UI.Xaml.Input;
using Gpt.Labs.Helpers;
using Gpt.Labs.ViewModels;
using Gpt.Labs.Controls.Markdown;

namespace Gpt.Labs.Controls
{
    public sealed class OpenAiMessageControl : BaseControl
    {
        #region Fields

        private Grid rootGrid;

        private MarkdownTextBlock textBlock;

        private AppBarButton copyButton;

        private MenuFlyoutItem shareButton;

        private MenuFlyoutItem deleteButton;

        #endregion

        #region Constructors

        public OpenAiMessageControl()
        {
            this.DefaultStyleKey = typeof(OpenAiMessageControl);
        }

        #endregion

        #region Properties

        private MessagesListViewModel ParentViewViewModel => this.GetParent<MessagesControl>()?.ViewModel;

        private OpenAIMessage ViewModel => this.DataContext as OpenAIMessage;

        #endregion

        #region Private Methods

        protected override void OnApplyTemplate()
        {
            this.rootGrid = (Grid)GetTemplateChild("RootGrid");
            this.textBlock = (MarkdownTextBlock)GetTemplateChild("MessageTextBlock");
            this.copyButton = (AppBarButton)GetTemplateChild("Copy");
            this.shareButton = (MenuFlyoutItem)GetTemplateChild("Share");
            this.deleteButton = (MenuFlyoutItem)GetTemplateChild("Delete");

            if (this.rootGrid != null)
            {
                this.rootGrid.PointerEntered -= OnRootGridPointerEntered;
                this.rootGrid.PointerEntered += OnRootGridPointerEntered;

                this.rootGrid.PointerExited -= OnRootGridPointerExited;
                this.rootGrid.PointerExited += OnRootGridPointerExited;
            }

            if (this.textBlock != null)
            {
                this.textBlock.LinkClicked -= this.OnMarkdownTextBlockLinkClicked;
                this.textBlock.LinkClicked += this.OnMarkdownTextBlockLinkClicked;
                //this.textBlock.ImageResolving -= this.OnMarkdownTextBlockImageResolving;
                //this.textBlock.ImageResolving += this.OnMarkdownTextBlockImageResolving;
                this.textBlock.ImageClicked -= this.OnMarkdownTextBlockImageClicked;
                this.textBlock.ImageClicked += this.OnMarkdownTextBlockImageClicked;

                this.textBlock.SetRenderer<ExtendedMarkdownRenderer>();
            }

            if (this.copyButton != null)
            {
                this.copyButton.Click -= this.OnCopyButtonClick;
                this.copyButton.Click += this.OnCopyButtonClick;
            }

            if (this.shareButton != null)
            {
                this.shareButton.Click -= this.OnShareButtonClick;
                this.shareButton.Click += this.OnShareButtonClick;
            }

            if (this.deleteButton != null)
            {
                this.deleteButton.Click -= this.OnDeleteButtonClick;
                this.deleteButton.Click += this.OnDeleteButtonClick;
            }

            base.OnApplyTemplate();
        }

        private void OnRootGridPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.ParentViewViewModel.HoveredElement = this.ViewModel;
            VisualStateManager.GoToState(this, "PointerEntered", true);
        }

        private void OnRootGridPointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.ParentViewViewModel.HoveredElement = null;
            VisualStateManager.GoToState(this, "PointerExited", true);
        }

        private async void OnCopyButtonClick(object sender, RoutedEventArgs e)
        {
            await this.ParentViewViewModel.CopyMessages(this.ViewModel);
        }

        private void OnShareButtonClick(object sender, RoutedEventArgs e)
        {
            this.ParentViewViewModel.ShareMessages(this.ViewModel);
        }

        private async void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            await this.ParentViewViewModel.DeleteMessages(true, this.ViewModel);
        }

        private async void OnMarkdownTextBlockLinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
            {
                await Launcher.LaunchUriAsync(link);
            }
        }

        private void OnMarkdownTextBlockImageResolving(object sender, ImageResolvingEventArgs e)
        {
            e.Image = new BitmapImage(new Uri(e.Url));
            e.Handled = true;
        }

        private async void OnMarkdownTextBlockImageClicked(object sender, LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
            {
                await Launcher.LaunchUriAsync(link);
            }
        }

        #endregion
    }
}
