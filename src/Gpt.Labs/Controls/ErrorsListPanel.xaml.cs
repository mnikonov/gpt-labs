using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace Gpt.Labs.Controls
{
    public sealed partial class ErrorsListPanel : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(List<string>),
            typeof(ErrorsListPanel),
            new PropertyMetadata(null, null));

        #endregion

        #region Constructors

        public ErrorsListPanel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public List<string> ViewModel
        {
            get => (List<string>)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        #endregion
    }
}
