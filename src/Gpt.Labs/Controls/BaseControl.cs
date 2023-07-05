using Gpt.Labs.Helpers;
using Microsoft.UI.Xaml.Controls;

namespace Gpt.Labs.Controls
{
    public abstract class BaseUserControl : UserControl
    {
        #region Properties

        public BasePage RootPage => this.GetParent<BasePage>();

        public MainWindow Window => RootPage?.Window;

        #endregion
    }
}
