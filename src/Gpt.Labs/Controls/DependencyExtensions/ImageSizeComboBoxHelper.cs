
using Gpt.Labs.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace Gpt.Labs.Controls.DependencyExtensions;

internal class ImageSizeComboBoxHelper : DependencyObject
{
    #region Fields

    public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
        "Settings",
        typeof(OpenAISettings),
        typeof(DependencyObject),
        new PropertyMetadata(false, OnApplySettingsHandler));

    #endregion

    #region Public Methods

    public static OpenAISettings GetSettings(DependencyObject element)
    {
        return (OpenAISettings)element.GetValue(SettingsProperty);
    }

    public static void SetSettings(DependencyObject element, OpenAISettings value)
    {
        element.SetValue(SettingsProperty, value);
    }

    #endregion

    #region Private Methods

    private static void OnApplySettingsHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var comboBox = (ComboBox)d;

        if (e.NewValue is OpenAIImageSettings settings)
        {
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(OpenAIImageSettings.ModelId))
                {
                    SetItemsSource(comboBox, settings);
                }
            };

            SetItemsSource(comboBox, settings);
        }
    }

    private static void SetItemsSource(ComboBox comboBox, OpenAIImageSettings settings)
    {
        var source = !string.IsNullOrEmpty(settings.ModelId) && Helpers.Convert.ImageSizes.ContainsKey(settings.ModelId) ? Helpers.Convert.ImageSizes[settings.ModelId] : [];
        var size = settings.Size;

        comboBox.ItemsSource = source;
        if (!comboBox.IsLoaded)
        {
            void loaded(object sender, RoutedEventArgs e)
            {
                comboBox.Loaded -= loaded;

                comboBox.SelectedItem = source.SingleOrDefault(p => p.Value == size);
            }

            comboBox.Loaded += loaded;
        }
        else
        {
            comboBox.SelectedItem = source.SingleOrDefault(p => p.Value == size);
        }
    }


    #endregion
}
