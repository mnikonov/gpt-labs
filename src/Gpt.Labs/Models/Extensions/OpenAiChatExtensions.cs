using Gpt.Labs.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gpt.Labs.Models.Extensions
{
    public static class OpenAiChatExtensions
    {
        #region Public Methods

        public static TSettings GetSettings<TSettings>(this OpenAIChat chat) where TSettings : OpenAISettings
        {
            return (TSettings)chat.Settings;
        }

        #endregion
    }
}
