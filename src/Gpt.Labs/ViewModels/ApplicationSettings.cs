using Gpt.Labs.Models.Attributes;
using Gpt.Labs.Models.Base;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;

namespace Gpt.Labs.ViewModels;

public class ApplicationSettings : ObservableObject
{
    #region Fields

    private static readonly object SyncRoot = new();

    private static volatile ApplicationSettings _instance;

    private bool _launchOnStartup;

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
            if (_instance != null)
            {
                return _instance;
            }

            lock (SyncRoot)
            {
                _instance ??= new ApplicationSettings();
            }

            return _instance;
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
        get => (ElementTheme)Get((int)ElementTheme.Default);
        set => Set((int)value);
    }

    public string OpenAIOrganization
    {
        get => Get(string.Empty);
        set => Set(value);
    }

    public string OpenAIApiKey
    {
        get => Get(string.Empty);
        set => Set(value);
    }

    public bool LaunchOnStartup
    {
        get => _launchOnStartup;

        set
        {
            if (_launchOnStartup != value)
            {
                _launchOnStartup = value;
                RaisePropertyChanged();
            }
        }
    }

    #endregion

    #region Public Methods

    public IAsyncOperation<StartupTask> GetStartupTask()
    {
        return StartupTask.GetAsync("GPTLabsStartupTask");
    }

    public async Task InitExtraSettings()
    {
        var startup = await GetStartupTask();

        _launchOnStartup = startup.State == StartupTaskState.Enabled;
    }

    public T Get<T>(T defaultValue, [CallerMemberName] string propertyName = null)
    {
        if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(propertyName)
            || ApplicationData.Current.LocalSettings.Values[propertyName] is not T)
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
        RaisePropertyChanged(propertyName);

        return true;
    }

    #endregion
}
