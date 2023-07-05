namespace Gpt.Labs.Helpers.Extensions
{
    using Gpt.Labs.Controls.Dialogs.Base;
    using Gpt.Labs.Models.Exceptions;
    using Microsoft.UI.Text;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Documents;
    using System;

    public static class ContentDialogExtensions
    {
        #region Public Methods

        public static ContentDialog CreateYesNoDialog(this Window window, string titleResourceKey, string descriptionResourceKey, params object[] descriptionArgs)
        {
            return new ContentDialogBase(window)
            {
                Title = App.ResourceLoader.GetString("DialogTitle/" + titleResourceKey),
                Content = new TextBlock { Text = string.Format(App.ResourceLoader.GetString("DialogDescription/" + descriptionResourceKey), descriptionArgs), TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Normal },
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = App.ResourceLoader.GetString("Yes"),
                SecondaryButtonText = App.ResourceLoader.GetString("No")
            };
        }

        public static ContentDialog CreateExceptionDialog(this Window window, Exception ex)
        {
            var title = App.ResourceLoader.GetString("DialogTitle/Error");
            var content = new TextBlock { TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Normal };

            content.Inlines.Add(new Run { Text = ex.Message });

#if DEBUG
            content.Inlines.Add(new LineBreak());
            content.Inlines.Add(new LineBreak());
            content.Inlines.Add(new Run { Text = ex.StackTrace });
#endif

            return new ContentDialogBase(window)
            {
                Title = title,
                Content = content,
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = false,
                PrimaryButtonText = App.ResourceLoader.GetString("Ok"),
            };
        }

        public static ContentDialog CreateErrorDialog(this Window window, OpenAiException error)
        {
            var title = App.ResourceLoader.GetString("DialogTitle/Error");
            var content = new TextBlock { TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Normal };

            content.Inlines.Add(new Run { Text = error.Message });


            return new ContentDialogBase(window)
            {
                Title = title,
                Content = content,
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = false,
                PrimaryButtonText = App.ResourceLoader.GetString("Ok"),
            };
        }

        public static ContentDialog CreateOkDialog(this Window window, string titleResourceKey, string descriptionResourceKey, params object[] descriptionArgs)
        {
            return new ContentDialogBase(window)
            {
                Title = App.ResourceLoader.GetString("DialogTitle/" + titleResourceKey),
                Content = new TextBlock { Text = string.Format(App.ResourceLoader.GetString("DialogDescription/" + descriptionResourceKey), descriptionArgs), TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Normal },
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = false,
                PrimaryButtonText = App.ResourceLoader.GetString("Ok"),
            };
        }

        public static ContentDialog CreateDialog(
            this Window window,
            string titleResourceKey,
            string descriptionResourceKey,
            string primaryButtonTextKey,
            string secondaryButtonTextKey,
            string closeButtonTextKey,
            params object[] descriptionArgs)
        {
            var isPrimaryButtonEnabled = !string.IsNullOrEmpty(primaryButtonTextKey);
            var isSecondaryButtonEnabled = !string.IsNullOrEmpty(secondaryButtonTextKey);
            var isCloseButtonEnabled = !string.IsNullOrEmpty(closeButtonTextKey);

            var primaryButtonText = isPrimaryButtonEnabled ? App.ResourceLoader.GetString(primaryButtonTextKey) : string.Empty;
            var secondaryButtonText = isSecondaryButtonEnabled ? App.ResourceLoader.GetString(secondaryButtonTextKey) : string.Empty;
            var closeButtonText = isCloseButtonEnabled ? App.ResourceLoader.GetString(closeButtonTextKey) : string.Empty;

            return new ContentDialogBase(window)
            {
                Title = App.ResourceLoader.GetString("DialogTitle/" + titleResourceKey),
                Content = new TextBlock { Text = string.Format(App.ResourceLoader.GetString("DialogDescription/" + descriptionResourceKey), descriptionArgs), TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Normal },
                IsPrimaryButtonEnabled = isPrimaryButtonEnabled,
                IsSecondaryButtonEnabled = isSecondaryButtonEnabled,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                CloseButtonText = closeButtonText
            };
        }

#endregion
    }
}
