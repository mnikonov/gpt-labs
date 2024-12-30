using CommunityToolkit.WinUI.UI.Controls;
using Gpt.Labs.Controls.Extensions;
using Gpt.Labs.Helpers;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.System;

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
            DefaultStyleKey = typeof(OpenAiMessageControl);
        }

        #endregion

        #region Properties

        private MessagesListViewModel ParentViewViewModel => this.GetParent<MessagesControl>()?.ViewModel;

        private OpenAIMessage ViewModel => DataContext as OpenAIMessage;

        #endregion

        #region Private Methods

        protected override void OnApplyTemplate()
        {
            rootGrid = (Grid)GetTemplateChild("RootGrid");
            textBlock = (MarkdownTextBlock)GetTemplateChild("MessageTextBlock");
            copyButton = (AppBarButton)GetTemplateChild("Copy");
            shareButton = (MenuFlyoutItem)GetTemplateChild("Share");
            deleteButton = (MenuFlyoutItem)GetTemplateChild("Delete");

            if (rootGrid != null)
            {
                rootGrid.PointerEntered -= OnRootGridPointerEntered;
                rootGrid.PointerEntered += OnRootGridPointerEntered;

                rootGrid.PointerExited -= OnRootGridPointerExited;
                rootGrid.PointerExited += OnRootGridPointerExited;
            }

            if (textBlock != null)
            {
                textBlock.LinkClicked -= OnMarkdownTextBlockLinkClicked;
                textBlock.LinkClicked += OnMarkdownTextBlockLinkClicked;
                //this.textBlock.ImageResolving -= this.OnMarkdownTextBlockImageResolving;
                //this.textBlock.ImageResolving += this.OnMarkdownTextBlockImageResolving;
                textBlock.ImageClicked -= OnMarkdownTextBlockImageClicked;
                textBlock.ImageClicked += OnMarkdownTextBlockImageClicked;

                //textBlock.SetRenderer<ExtendedMarkdownRenderer>();
            }

            if (copyButton != null)
            {
                copyButton.Click -= OnCopyButtonClick;
                copyButton.Click += OnCopyButtonClick;
            }

            if (shareButton != null)
            {
                shareButton.Click -= OnShareButtonClick;
                shareButton.Click += OnShareButtonClick;
            }

            if (deleteButton != null)
            {
                deleteButton.Click -= OnDeleteButtonClick;
                deleteButton.Click += OnDeleteButtonClick;
            }

            base.OnApplyTemplate();
        }

        private void OnRootGridPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ParentViewViewModel.HoveredElement = ViewModel;
            VisualStateManager.GoToState(this, "PointerEntered", true);
        }

        private void OnRootGridPointerExited(object sender, PointerRoutedEventArgs e)
        {
            ParentViewViewModel.HoveredElement = null;
            VisualStateManager.GoToState(this, "PointerExited", true);
        }

        private async void OnCopyButtonClick(object sender, RoutedEventArgs e)
        {
            await ParentViewViewModel.CopyMessages(ViewModel);
        }

        private void OnShareButtonClick(object sender, RoutedEventArgs e)
        {
            ParentViewViewModel.ShareMessages(ViewModel);
        }

        private async void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                await ParentViewViewModel.DeleteMessages(true, ViewModel);
            });
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
