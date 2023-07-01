using CommunityToolkit.Common.Parsers.Markdown.Blocks;
using CommunityToolkit.Common.Parsers.Markdown.Render;
using CommunityToolkit.Common.Parsers.Markdown;
using CommunityToolkit.WinUI.UI.Controls.Markdown.Render;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System.Linq;
using Windows.UI;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using CommunityToolkit.WinUI.UI;
using Gpt.Labs.Helpers;
using OpenAI.Chat;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;

namespace Gpt.Labs.Controls.Markdown
{
    public class ExtendedMarkdownRenderer : MarkdownRenderer
    {
        public ExtendedMarkdownRenderer(MarkdownDocument document, ILinkRegister linkRegister, IImageResolver imageResolver, ICodeBlockResolver codeBlockResolver)
            : base(document, linkRegister, imageResolver, codeBlockResolver)
        {
        }

        protected override void RenderCode(CodeBlock element, IRenderContext context)
        {
            // Renders the Code Block in the standard fashion.
            base.RenderCode(element, context);
         
            // Grab the Local context and cast it.
            var localContext = context as UIElementCollectionRenderContext;
            var collection = localContext?.BlockUIElementCollection;

            // Don't go through with it, if there is an issue with the context or collection.
            if (localContext == null || collection?.Any() != true)
            {
                return;
            }

            var scrollViewer = collection.Last() as ScrollViewer;

            if (scrollViewer == null)
            {
                return;
            }

            collection.Remove(scrollViewer);

            // Unify all Code Language headers for C#.
            var language = element.CodeLanguage?.ToUpper();

            if (!string.IsNullOrEmpty(language))
            {
                switch (language)
                {
                    case "CSHARP":
                    case "CS":
                        language = "C#";
                        break;
                }
            }

            var copyButton = new AppBarButton
            {
                Label = "Copy",
                Icon = new FontIcon() { Glyph = "\uE8C8" },
            };

            ToolTipService.SetToolTip(copyButton, new ToolTip() { Content = App.ResourceLoader.GetString("CopyToolTip/Content") });

            copyButton.Click += (s, e) =>
            {
                var content = new DataPackage();
                content.SetText(element.Text.ConvertCrLfToLf());
                Clipboard.SetContent(content);
            };

            var shareButton = new AppBarButton
            {
                Label = "Share",
                Icon = new FontIcon() { Glyph = "\uE72D" }
            };

            ToolTipService.SetToolTip(shareButton, new ToolTip() { Content = App.ResourceLoader.GetString("ShareToolTip/Content") });

            shareButton.Click += (s, e) =>
            {
                var windows = ((UIElement)s).GetParent<BasePage>()?.Window;

                if (windows == null)
                {
                    return;
                }

                windows.SetShareContent(new ShareContent
                {
                    Title = language,
                    Message = element.Text.ConvertCrLfToLf()
                });

                windows.ShowShareUI();
            };

            var panel = new TitledContentPanel()
            {
                Background = scrollViewer.Background,
                BorderBrush = scrollViewer.BorderBrush,
                BorderThickness = scrollViewer.BorderThickness,
                Margin = scrollViewer.Margin,
                Padding = new Thickness(0),
                Title = language
            };
                        
            scrollViewer.Background = null; 
            scrollViewer.BorderBrush = null;
            scrollViewer.BorderThickness = new Thickness(0);
            scrollViewer.Margin = new Thickness(0);

            panel.PrimaryCommands.Add(copyButton);
            panel.PrimaryCommands.Add(shareButton);

            panel.Content = scrollViewer;

            collection.Add(panel);
        }
    }
}
