using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Gpt.Labs.Controls
{
    public class OpenAiSettingsControl : Control
    {
        #region Fields

        public static readonly DependencyProperty ChatSettingsProperty = DependencyProperty.Register(
            nameof(ChatSettings),
            typeof(OpenAISettings),
            typeof(OpenAiSettingsControl),
            new PropertyMetadata(null, null));

        public static readonly DependencyProperty SupportedAiModelsProperty = DependencyProperty.Register(
            nameof(SupportedAiModels),
            typeof(IReadOnlyCollection<string>),
            typeof(OpenAiSettingsControl),
            new PropertyMetadata(null, null));

        #endregion

        #region Constructors

        public OpenAiSettingsControl()
        {
            this.DefaultStyleKey = typeof(OpenAiSettingsControl);
        }

        #endregion

        #region Properties

        public OpenAISettings ChatSettings
        {
            get => (OpenAISettings)this.GetValue(ChatSettingsProperty);
            set
            {
                switch (value.Type)
                {
                    case OpenAIChatType.Chat:
                        this.SupportedAiModels = ApplicationSettings.Instance.OpenAIModels.Where(p => p.Id.Contains("gpt")).OrderByDescending(p => p.CreatedAt).Select(p => p.Id).ToList().AsReadOnly();
                        break;
                }

                this.SetValue(ChatSettingsProperty, value);
            }
        }

        public IReadOnlyCollection<string> SupportedAiModels 
        {
            get => (IReadOnlyCollection<string>)this.GetValue(SupportedAiModelsProperty);
            private set => this.SetValue(SupportedAiModelsProperty, value);
        }

        #endregion
    }
}
