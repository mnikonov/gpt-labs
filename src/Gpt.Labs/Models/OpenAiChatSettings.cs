using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gpt.Labs.Models
{
    public class OpenAIChatSettings : OpenAISettings
    {
        #region Fields

        private string modelId;

        private string systemMessage;

        private int lastNMessagesToInclude = 5;

        private float temperature = 1;

        private float top_p = 1;

        private bool stream = true;

        private ICollection<OpenAIStop> stop;

        private int? max_tokens;

        private float presence_penalty = 0;

        private float frequency_penalty = 0;

        private ICollection<OpenAILogitBias> logit_bias;

        #endregion

        #region Properties

        /// <summary>
        /// string Required
        /// ID of the model to use. See the model endpoint compatibility table for details on which models work with the Chat API.
        /// https://platform.openai.com/docs/models/model-endpoint-compatibility
        /// </summary>
        [Required(ErrorMessage = "The 'Model' field is required")]
        [StringLength(250, ErrorMessage = "The 'Model' field must be a string with a maximum length of {1}.")]
        public string ModelId 
        { 
            get => this.modelId;
            set => this.Set(ref this.modelId, value);
        }

        [Required(ErrorMessage = "The 'System message' field is required")]
        [StringLength(2500, ErrorMessage = "The 'System message' field must be a string with a maximum length of {1}.")]
        public string SystemMessage 
        { 
            get => this.systemMessage;
            set => this.Set(ref this.systemMessage, value);
        }

        /// <summary>
        /// A list of messages describing the conversation so far.
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Value for 'Last N messages to include' must be between {1} and {2}.")]
        public int LastNMessagesToInclude
        { 
            get => this.lastNMessagesToInclude;
            set => this.Set(ref this.lastNMessagesToInclude, value);
        }

        /// <summary>
        /// number Optional Defaults to 1
        /// What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
        /// We generally recommend altering this or top_p but not both.
        /// </summary>
        [Range(0f, 2f, ErrorMessage = "Value for 'Temperature' must be between {1} and {2}.")]
        public float Temperature
        { 
            get => this.temperature;
            set => this.Set(ref this.temperature, value);
        }

        /// <summary>
        /// number Optional Defaults to 1
        /// An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered.
        /// We generally recommend altering this or temperature but not both.
        /// </summary>
        [Range(0f, 1f, ErrorMessage = "Value for 'Top_P' must be between {1} and {2}.")]
        public float TopP
        { 
            get => this.top_p;
            set => this.Set(ref this.top_p, value);
        }

        /// <summary>
        /// boolean Optional Defaults to false
        /// If set, partial message deltas will be sent, like in ChatGPT. Tokens will be sent as data-only server-sent events as they become available, with the stream terminated by a data: [DONE] message. See the OpenAI Cookbook for example code.
        /// https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events#Event_stream_format
        /// https://github.com/openai/openai-cookbook/blob/main/examples/How_to_stream_completions.ipynb
        /// </summary>
        public bool Stream
        { 
            get => this.stream;
            set => this.Set(ref this.stream, value);
        }

        /// <summary>
        /// integer Optional Defaults to inf
        /// The maximum number of tokens to generate in the chat completion.
        /// The total length of input tokens and generated tokens is limited by the model's context length.
        /// https://platform.openai.com/tokenizer
        /// </summary>
        public int? MaxTokens
        { 
            get => this.max_tokens;
            set => this.Set(ref this.max_tokens, value);
        }

        /// <summary>
        /// number Optional Defaults to 0
        /// Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics.
        /// https://platform.openai.com/docs/api-reference/parameter-details
        /// </summary>
        [Range(-2f, 2f, ErrorMessage = "Value for 'Presence penalty' must be between {1} and {2}.")]
        public float PresencePenalty
        { 
            get => this.presence_penalty;
            set => this.Set(ref this.presence_penalty, value);
        }

        /// <summary>
        /// number Optional Defaults to 0
        /// Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.
        /// https://platform.openai.com/docs/api-reference/parameter-details
        /// </summary>
        [Range(-2f, 2f, ErrorMessage = "Value for 'Frequency penalty' must be between {1} and {2}.")]
        public float FrequencyPenalty
        { 
            get => this.frequency_penalty;
            set => this.Set(ref this.frequency_penalty, value);
        }

        /// <summary>
        /// string or array Optional Defaults to null
        /// Up to 4 sequences where the API will stop generating further tokens.
        /// </summary>
        [MaxLength(4)]
        public ICollection<OpenAIStop> Stop
        {
            get => this.stop;
            set => this.Set(ref this.stop, value);
        }

        /// <summary>
        /// map Optional Defaults to null
        /// Modify the likelihood of specified tokens appearing in the completion.
        /// Accepts a json object that maps tokens (specified by their token ID in the tokenizer) to an associated bias value from -100 to 100. 
        /// Mathematically, the bias is added to the logits generated by the model prior to sampling. 
        /// The exact effect will vary per model, but values between -1 and 1 should decrease or increase likelihood of selection; values like -100 or 100 should result in a ban or exclusive selection of the relevant token.
        /// </summary>
        public ICollection<OpenAILogitBias> LogitBias
        {
            get => this.logit_bias;
            set => this.Set(ref this.logit_bias, value);
        }

        #endregion
    }
}
