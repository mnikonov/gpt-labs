using Gpt.Labs.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gpt.Labs.Models
{
    public class OpenAIApiSettings : ObservableValidationObject
    {
        #region Fields

        private string organization;

        private string apiKey;

        #endregion

        #region Constructors

        public OpenAIApiSettings()
        {

        }

        public OpenAIApiSettings(string organization, string apiKey)
        {
            this.organization = organization;
            this.apiKey = apiKey;
        }

        #endregion

        #region Properties

        [Required(ErrorMessage = "The 'Organization ID' field is required")]
        [JsonPropertyOrder(1)]
        public string Organization
        {
            get => this.organization;
            set => this.Set(ref this.organization, value);
        }

        [Required(ErrorMessage = "The 'Secret Key' field is required")]
        [JsonPropertyOrder(2)]
        public string ApiKey
        {
            get => this.apiKey;
            set => this.Set(ref this.apiKey, value);
        }

        #endregion
    }
}
