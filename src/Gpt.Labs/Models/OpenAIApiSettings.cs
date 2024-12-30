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

        [Required(ErrorMessage = "The 'Secret Key' field is required")]
        [StringLength(250, ErrorMessage = "The 'Secret Key' field must be a string with a maximum length of {1}.")]
        [RegularExpression(@"^sk-.*$", ErrorMessage = "The field 'Secret Key' must start with 'sk-' prefix.")]
        [JsonPropertyOrder(1)]
        public string ApiKey
        {
            get => apiKey;
            set => Set(ref apiKey, value);
        }

        [StringLength(50, ErrorMessage = "The 'Organization ID' field must be a string with a maximum length of {1}.")]
        [RegularExpression(@"^org-.*$", ErrorMessage = "The field 'Organization ID' must start with 'org-' prefix.")]
        [JsonPropertyOrder(2)]
        public string Organization
        {
            get => organization;
            set => Set(ref organization, value);
        }

        #endregion
    }
}
