using Microsoft.UI.Xaml;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using Windows.Storage;
using System.Linq;
using Gpt.Labs.Models.Attributes;
using Gpt.Labs.Models.Base;
using OpenAI.Models;
using System.Collections.Generic;

namespace Gpt.Labs.ViewModels
{
    public class ApplicationSettings : ObservableObject
    {
        #region Fields

        private static readonly object SyncRoot = new object();

        private static volatile ApplicationSettings instance;

        #endregion

        #region Constructors

        private ApplicationSettings()
        {
        }

        #endregion

        #region Properties

        public static ApplicationSettings Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                lock (SyncRoot)
                {
                    if (instance == null)
                    {
                        instance = new ApplicationSettings();
                    }
                }

                return instance;
            }
        }

        public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            {
                Modifiers =
                    {
                        (JsonTypeInfo jsonTypeInfo) =>
                        {
                            var toRemove = jsonTypeInfo.Properties.Where(p => p.AttributeProvider.IsDefined(typeof(ExternalJsonIgnoreAttribute), false)).ToList();

                            foreach (var item in toRemove)
                            {
                                jsonTypeInfo.Properties.Remove(item);
                            }
                        }
                    }
            }
        };

        public ElementTheme AppTheme
        {
            get => (ElementTheme)this.Get((int)ElementTheme.Default);
            set => this.Set((int)value);
        }
                
        public IReadOnlyList<Model> OpenAIModels { get; set; }

        public string OpenAIOrganization
        {
            get => this.Get(string.Empty);
            set => this.Set(value);
        }

        public string OpenAIApiKey
        {
            get => this.Get(string.Empty);
            set => this.Set(value);
        }

        #endregion

        #region Public Methods


        public T Get<T>(T defaultValue, [CallerMemberName] string propertyName = null)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(propertyName)
                || !(ApplicationData.Current.LocalSettings.Values[propertyName] is T))
            {
                ApplicationData.Current.LocalSettings.Values[propertyName] = defaultValue;
            }

            return (T)ApplicationData.Current.LocalSettings.Values[propertyName];
        }

        public bool Set<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            var oldValue = ApplicationData.Current.LocalSettings.Values[propertyName];

            if (Equals(oldValue, newValue))
            {
                return false;
            }

            ApplicationData.Current.LocalSettings.Values[propertyName] = newValue;
            this.RaisePropertyChanged(propertyName);

            return true;
        }

        #endregion
    }
}
