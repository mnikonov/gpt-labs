using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using Windows.Foundation.Collections;

namespace Gpt.Labs.Controls
{
    public partial class TitledContentPanel : ContentControl
    {
        #region Fields

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(TitledContentPanel),
            new PropertyMetadata(string.Empty));

        private readonly CommandBar commandBar = new ();

        #endregion

        #region Constructors

        public TitledContentPanel()
        {
            this.DefaultStyleKey = typeof(TitledContentPanel);

            this.commandBar.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("ms-appx:///Styles/CommandBarMediumResources.xaml")});
        }

        #endregion

        #region Properties

        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }
        
        public IObservableVector<ICommandBarElement> PrimaryCommands => this.commandBar.PrimaryCommands;

        public IObservableVector<ICommandBarElement> SecondaryCommands => this.commandBar.SecondaryCommands;

        #endregion

        #region Private Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var panel = (Border)this.GetTemplateChild("ActionsPanel");
            panel.Child = this.commandBar;
        }

        #endregion
    }
}
