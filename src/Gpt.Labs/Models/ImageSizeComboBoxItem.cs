using Gpt.Labs.Models.Enums;

namespace Gpt.Labs.Models;

public class ImageSizeComboBoxItem
{
    public ImageSizeComboBoxItem(OpenAIImageSize value, string caption)
    {
        Value = value;
        Caption = caption;
    }

    public string Caption { get; set; }
    public OpenAIImageSize Value { get; set; }
}
