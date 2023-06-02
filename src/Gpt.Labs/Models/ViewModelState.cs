using Gpt.Labs.ViewModels;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Gpt.Labs.Models
{
    public class ViewModelState : Dictionary<string, string>
    {
        #region Public Methods

        public bool ContainsKey(Type key)
        {
            return this.ContainsKey(key.Name);
        }

        public bool ContainsKey<TValue>()
        {
            return this.ContainsKey(typeof(TValue).Name);
        }

        public TValue GetValue<TValue>()
            where TValue : class
        {
            return this.GetValue<TValue>(typeof(TValue).Name);
        }

        public TValue GetValue<TValue>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Parameter cannot be null or empty", nameof(key));
            }

            if (this.ContainsKey(key))
            {
                return JsonSerializer.Deserialize<TValue>(this[key], ApplicationSettings.Instance.SerializerOptions);
            }
            
            return default(TValue);
        }

        public void SetValue<TValue>(TValue value)
        {
            this.SetValue(typeof(TValue).Name, value);
        }

        public void SetValue<TValue>(string key, TValue value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Parameter cannot be null or empty", nameof(key));
            }

            this[key] = JsonSerializer.Serialize<TValue>(value, ApplicationSettings.Instance.SerializerOptions);
        }

        #endregion
    }
}
