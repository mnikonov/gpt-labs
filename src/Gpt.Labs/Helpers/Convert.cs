using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Microsoft.UI.Xaml.Markup;
using System.Collections.Generic;
using Windows.UI;

namespace Gpt.Labs.Helpers;

public static class Convert
{
    public static readonly Dictionary<string, List<ImageSizeComboBoxItem>> ImageSizes = new()
    {
        ["dall-e-2"] = [
            new ImageSizeComboBoxItem(OpenAIImageSize.Small, "256x256"),
            new ImageSizeComboBoxItem(OpenAIImageSize.Medium, "512x512"),
            new ImageSizeComboBoxItem(OpenAIImageSize.Large, "1024x1024")
        ],
        ["dall-e-3"] = [
            new ImageSizeComboBoxItem(OpenAIImageSize.Small, "1024x1024"),
            new ImageSizeComboBoxItem(OpenAIImageSize.Medium, "1792x1024"),
            new ImageSizeComboBoxItem(OpenAIImageSize.Large, "1024x1792")
        ],
    };

    public static Color ToColor(this string color)
    {
        return (Color)XamlBindingHelper.ConvertValue(typeof(Color), color);
    }

    public static string ToImageSize(this string modelId, OpenAIImageSize size)
    {
        return !ImageSizes.ContainsKey(modelId)
            ? null
            : size switch
            {
                OpenAIImageSize.Small => ImageSizes[modelId][0].Caption,
                OpenAIImageSize.Medium => ImageSizes[modelId][1].Caption,
                OpenAIImageSize.Large => ImageSizes[modelId][2].Caption,
                _ => string.Empty,
            };
    }
}
