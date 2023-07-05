using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using Gpt.Labs.Models.Exceptions;

namespace Gpt.Labs.Models.Extensions
{
    public static class HttpRequestExceptionExtensions
    {
        private static readonly Regex jsonRx = new Regex("\\{(?:[^{}]|(?<o>\\{)|(?<-o>\\})|(?<c>\\}),?)*(?(o)(?!))\\}", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static OpenAiException ToOpenAiError(this HttpRequestException exception)
        {
            var match = jsonRx.Match(exception.Message);

            if (match.Success) 
            {
                var error = JsonDocument.Parse(match.Value).RootElement.GetProperty("error");

                OpenAiException openAiException = null;

                if (error.TryGetProperty("message", out var message))
                {
                    openAiException = new OpenAiException(message.GetString());
                }
                else
                {
                    return null;
                }

                if (error.TryGetProperty("type", out var type))
                {
                    openAiException.Type = type.GetString();
                }

                if (error.TryGetProperty("param", out var param))
                {
                    openAiException.Param = param.GetString();
                }

                if (error.TryGetProperty("code", out var code))
                {
                    openAiException.Code = code.GetString();
                }

                return openAiException;
            }

            return null;
        }
    }
}
