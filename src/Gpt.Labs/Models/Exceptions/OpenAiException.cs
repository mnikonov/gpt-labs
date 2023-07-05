using System;

namespace Gpt.Labs.Models.Exceptions
{
    public class OpenAiException : Exception
    {
        public OpenAiException(string message) 
         : base(message)
        { 
        }

        public string Type { get; set; }

        public string Param { get; set; }

        public string Code { get; set; }
    }
}
