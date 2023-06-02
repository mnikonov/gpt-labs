﻿using Gpt.Labs.Models.Enums;

namespace Gpt.Labs.Models
{
    public class OpenAIImageSettings : OpenAISettings
    {
        #region Fields

        private OpenAIImageSize size = OpenAIImageSize.Large;

        #endregion

        #region Properties

        /// <summary>
        /// string Optional Defaults to 1024x1024
        /// The size of the generated images. Must be one of 256x256, 512x512, or 1024x1024.
        /// </summary>
        public OpenAIImageSize Size 
        { 
            get => this.size;
            set => this.Set(ref this.size, value);
        }

        #endregion
    }
}
