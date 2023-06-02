using Gpt.Labs.Models.Enums;
using OpenAI.Images;
using Windows.Storage;

namespace Gpt.Labs.Models.Extensions
{
    public static class OpenAiImageSettingsExtensions
    {
        public static ImageGenerationRequest ToImageGenerationRequest(this OpenAIImageSettings settings, string userMessage)
        {
            return new ImageGenerationRequest(
                userMessage, 
                settings.N,
                settings.Size.ConvertImageType(),
                settings.User,
                ResponseFormat.Url);
        }

        public static ImageVariationRequest ToImageVariationRequest(this OpenAIImageSettings settings, StorageFile storageFile)
        {
            return new ImageVariationRequest(
                storageFile.Path, 
                settings.N,
                settings.Size.ConvertImageType(),
                settings.User,
                ResponseFormat.Url);
        }

        private static ImageSize ConvertImageType(this OpenAIImageSize size)
        {
            switch (size)
            {
                case OpenAIImageSize.Small:
                    return ImageSize.Small;
                case OpenAIImageSize.Medium:
                    return ImageSize.Medium;
                case OpenAIImageSize.Large:
                    return ImageSize.Large;
            }

            return ImageSize.Large;
        }
    }
}
