using Gpt.Labs.Models.Extensions;
using OpenAI;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gpt.Labs.Helpers.Extensions
{
    public static class OpenAIClientExtensions
    {
        public static async Task<TResult> WrapAction<TResult>(this OpenAIClient client, Func<OpenAIClient, Task<TResult>> action)
        {
            try
            {
                return await action(client);
            }
            catch (HttpRequestException ex)
            {
                var error = ex.ToOpenAiError();

                if (error != null)
                {
                    throw error;
                }

                throw;
            }
        }

        public static async Task<TResult> WrapAction<TResult>(this OpenAIClient client, Func<OpenAIClient, CancellationToken, Task<TResult>> action, CancellationToken token)
        {
            try
            {
                return await action(client, token);
            }
            catch (HttpRequestException ex)
            {
                var error = ex.ToOpenAiError();

                if (error != null)
                {
                    throw error;
                }

                throw;
            }
        }
    }
}
