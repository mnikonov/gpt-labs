using Gpt.Labs.Models;
using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace Gpt.Labs.Controls
{
    public class OpenAiSettingsControl : BaseControl
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
            set => this.SetValue(ChatSettingsProperty, value);
        }

        public IReadOnlyCollection<string> SupportedAiModels 
        {
            get => (IReadOnlyCollection<string>)this.GetValue(SupportedAiModelsProperty);
            set => this.SetValue(SupportedAiModelsProperty, value);
        }

        #endregion
    }
}
