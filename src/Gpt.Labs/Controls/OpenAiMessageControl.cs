using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gpt.Labs.Models;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.System;
using Windows.ApplicationModel.DataTransfer;
using Gpt.Labs.Helpers.Extensions;
using Microsoft.UI.Xaml.Input;
using System.Diagnostics;
using Gpt.Labs.Helpers;

namespace Gpt.Labs.Controls
{
    public sealed class OpenAiMessageControl : Control
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
            VisualStateManager.GoToState(this, "PointerOver", false);
        }

        private void OnRootGridPointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", false);
        }

        private async void OnCopyButtonClick(object sender, RoutedEventArgs e)
        {
            await this.GetParent<MessagesPage>().ViewModel.Result.CopyMessages((OpenAIMessage)this.DataContext);
        }

        private void OnShareButtonClick(object sender, RoutedEventArgs e)
        {
            this.GetParent<MessagesPage>().ViewModel.Result.ShareMessages((OpenAIMessage)this.DataContext);
        }

        private async void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            await this.GetParent<MessagesPage>().ViewModel.Result.DeleteMessages((OpenAIMessage)this.DataContext);
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
