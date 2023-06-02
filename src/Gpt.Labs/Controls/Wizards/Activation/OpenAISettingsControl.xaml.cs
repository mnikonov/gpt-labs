using Gpt.Labs.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Gpt.Labs.Controls.Wizards.Activation
{
    public sealed partial class OpenAISettingsControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(OpenAIApiSettings),
            typeof(OpenAISettingsControl),
            new PropertyMetadata(null, null));

        #endregion

        #region Constructors

        public OpenAISettingsControl(OpenAIApiSettings model)
        {
            this.ViewModel = model;

            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public OpenAIApiSettings ViewModel
        {
            get => (OpenAIApiSettings)this.GetValue(ViewModelProperty);
            private set => this.SetValue(ViewModelProperty, value);
        }

        #endregion
    }
}
