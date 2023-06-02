using System;
using System.ComponentModel.DataAnnotations;
using Gpt.Labs.Models.Base;

namespace Gpt.Labs.Models
{
    public class OpenAILogitBias : OpenAIToken
    {
        #region Fields

        private float bias;

        #endregion

        #region Private Fields

        [Required(ErrorMessage = "The 'Bias' field is required")]
        [Range(-100f, 100f, ErrorMessage = "Value for 'Bias' must be between {1} and {2}.")]
        public float Bias 
        { 
            get => this.bias;
            set => this.Set(ref this.bias, value);
        }

        #endregion 
    }
}
