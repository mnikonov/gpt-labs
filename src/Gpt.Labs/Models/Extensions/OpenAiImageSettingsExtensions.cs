using Gpt.Labs.Models.Enums;
using OpenAI.Images;
using OpenAI.Models;
using System;
using Windows.Storage;

namespace Gpt.Labs.Models.Extensions
{
    public static class OpenAiImageSettingsExtensions
    {
        public static ImageGenerationRequest ToImageGenerationRequest(this OpenAIImageSettings settings, OpenAIMessage userMessage)
        {
            return new ImageGenerationRequest(
                userMessage.Content,
                new Model(settings.ModelId),
                settings.N,
                quality: null, // TODO add this paramether to settings
                ImageResponseFormat.Url,
                settings.Size.ConvertImageSize(settings.ModelId),
                settings.User);
        }

        public static ImageVariationRequest ToImageVariationRequest(this OpenAIImageSettings settings, StorageFile storageFile)
        {
            return new ImageVariationRequest(
                storageFile.Path,
                settings.N,
                settings.Size.ConvertImageType(),
                settings.User,
                ImageResponseFormat.Url,
                new Model(settings.ModelId)
            );
        }

        private static ImageSize ConvertImageType(this OpenAIImageSize size)
        {
            return size switch
            {
                OpenAIImageSize.Small => ImageSize.Small,
                OpenAIImageSize.Medium => ImageSize.Medium,
                OpenAIImageSize.Large => ImageSize.Large,
                _ => ImageSize.Large,
            };
        }

        private static string ConvertImageSize(this OpenAIImageSize size, string modeId)
        {
            return modeId switch
            {
                "dall-e-2" => size switch
                {
                    OpenAIImageSize.Small => "256x256",
                    OpenAIImageSize.Medium => "512x512",
                    OpenAIImageSize.Large => "1024x1024",
                    _ => "1024x1024",
                },

                "dall-e-3" => size switch
                {
                    OpenAIImageSize.Small => "1024x1024",
                    OpenAIImageSize.Medium => "1792x1024",
                    OpenAIImageSize.Large => "1024x1792",
                    _ => "1024x1792",
                },

                _ => throw new ArgumentException($"Unable to determine image size for Model {modeId}"),
            };
        }
    }
}
