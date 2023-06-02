using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Gpt.Labs.Models.Extensions
{
    public static class HttpRequestExceptionExtensions
    {
        private static readonly Regex jsonRx = new Regex("\\{(?:[^{}]|(?<o>\\{)|(?<-o>\\})|(?<c>\\}),?)*(?(o)(?!))\\}", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static OpenAIError ToOpenAiError(this HttpRequestException exception)
        {
            var match = jsonRx.Match(exception.Message);

            if (match.Success) 
            {
                return JsonDocument.Parse(match.Value).RootElement.GetProperty("error").Deserialize<OpenAIError>();
            }

            return null;
        }
    }
}
