using Gpt.Labs.Controls.Dialogs;
using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models.Exceptions;
using Gpt.Labs.ViewModels.Base;
using Gpt.Labs.ViewModels.Collections;
using Gpt.Labs.ViewModels.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Gpt.Labs.ViewModels
{
    public class ChatsListViewModel : ListViewModel<OpenAIChat>
    {
        #region Fields

        private bool multiSelectModeEnabled;

        #endregion

        #region Constructors

        public ChatsListViewModel(Func<BasePage> getBasePage)
            : base(getBasePage)
        {
        }

        #endregion

        #region Properties

        public OpenAIChatType ChatType { get; private set; }

        public bool MultiSelectModeEnabled
        {
            get => multiSelectModeEnabled;
            set => Set(ref multiSelectModeEnabled, value);
        }

        #endregion

        #region Public Methods

        public async Task<SaveResult> AddEditChat(OpenAIChat chat)
        {
            var dialogModel = new OpenAIChat() { Title = chat?.Title, Type = ChatType, Position = 0 };

            OpenAISettings settings = null;

            switch (ChatType)
            {
                case OpenAIChatType.Chat:
                    var chatSettings = new OpenAIChatSettings
                    {
                        Type = ChatType,
                        SystemMessage = ((OpenAIChatSettings)chat?.Settings)?.SystemMessage,
                        ModelId = chat?.Settings?.ModelId
                    };
                    settings = chatSettings;
                    break;
                case OpenAIChatType.Image:
                    var imageSettings = new OpenAIImageSettings
                    {
                        Type = ChatType,
                        Size = ((OpenAIImageSettings)chat?.Settings)?.Size ?? OpenAIImageSize.Small,
                        ModelId = chat?.Settings?.ModelId
                    };
                    settings = imageSettings;
                    break;
            }

            settings.User = chat?.Settings.User;
            settings.OpenAIOrganization = chat?.Settings.OpenAIOrganization;

            dialogModel.Settings = settings;

            ContentDialogBase dialog = null;

            var models = await GetSupportedChatModels(dialogModel);

            if (models == null)
            {
                return SaveResult.Cancelled;
            }

            switch (ChatType)
            {
                case OpenAIChatType.Chat:
                    dialog = new EditChatDialog(Window, dialogModel, models);
                    break;
                case OpenAIChatType.Image:
                    dialog = new EditImageDialog(Window, dialogModel, models);
                    break;
            }

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return SaveResult.Cancelled;
            }

            if (chat != null)
            {
                chat.Title = dialogModel.Title;
                chat.Settings.User = settings.User;
                chat.Settings.OpenAIOrganization = settings.OpenAIOrganization;
                chat.Settings.ModelId = settings.ModelId;

                switch (ChatType)
                {
                    case OpenAIChatType.Chat:
                        ((OpenAIChatSettings)chat.Settings).SystemMessage = ((OpenAIChatSettings)settings).SystemMessage;
                        break;
                    case OpenAIChatType.Image:
                        ((OpenAIImageSettings)chat.Settings).Size = ((OpenAIImageSettings)settings).Size;
                        ((OpenAIImageSettings)chat.Settings).RaisePropertyChanged(nameof(OpenAIImageSettings.SizeCaption));
                        break;
                }
            }

            var saveResult = SaveResult.Cancelled;

            using (var context = new DataContext())
            {
                if (chat != null)
                {
                    context.Entry(chat).State = EntityState.Modified;
                    context.Entry(chat.Settings).State = EntityState.Modified;

                    saveResult = SaveResult.Edited;
                }
                else
                {
                    for (int i = 0; i < ItemsCollection.Count; i++)
                    {
                        var item = ItemsCollection[i];
                        item.Position = i + 1;
                        context.Entry(item).State = EntityState.Modified;
                    }

                    context.Add(dialogModel);

                    saveResult = SaveResult.Added;
                }

                await context.SaveChangesAsync();
            }

            if (chat == null)
            {
                ItemsCollection.Insert(0, dialogModel);
            }

            return saveResult;
        }

        public async Task DeleteChats(params OpenAIChat[] chats)
        {
            ContentDialog dialog = chats.Length == 1
                ? Window.CreateYesNoDialog("Confirm", "DeleteChat", chats[0].Title)
                : Window.CreateYesNoDialog("Confirm", "DeleteChats");
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                using (var context = new DataContext())
                {
                    foreach (var chat in chats)
                    {
                        for (int i = chat.Position + 1; i < ItemsCollection.Count; i++)
                        {
                            var item = ItemsCollection[i];
                            item.Position = i - 1;
                            context.Entry(item).State = EntityState.Modified;
                        }

                        context.Entry(chat).State = EntityState.Deleted;

                        ItemsCollection.Remove(chat);

                        if (SelectedElement != null && chat.Id == SelectedElement.Id)
                        {
                            SelectedElement = null;
                        }
                    }

                    await context.SaveChangesAsync();
                }

                foreach (var chat in chats)
                {
                    if (chat.Type == OpenAIChatType.Image)
                    {
                        var chatFolder = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(chat.Id.ToString());

                        if (chatFolder != null)
                        {
                            await chatFolder.DeleteAsync();
                        }
                    }
                }
            }
        }

        public async Task UpdateChatPosition(OpenAIChat chat)
        {
            using var context = new DataContext();
            var newIndex = ItemsCollection.IndexOf(chat);

            int from, to;

            if (newIndex > chat.Position)
            {
                from = chat.Position;
                to = newIndex;
            }
            else
            {
                from = newIndex;
                to = chat.Position;
            }

            for (int i = from; i <= to; i++)
            {
                var item = ItemsCollection[i];
                item.Position = i;
                context.Entry(item).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();
        }

        public void SelectChat(OpenAIChat chat)
        {
            SelectedElement = chat;
        }

        public override async Task LoadStateAsync(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            ChatType = mode == NavigationMode.New
                ? parameters.GetValue<OpenAIChatType>("chat-type")
                : state.GetValue<OpenAIChatType>(nameof(ChatType));

            using (var context = new DataContext())
            {
                var chats = await context.Chats.Include(p => p.Settings).AsNoTracking().Where(p => p.Type == ChatType).OrderBy(p => p.Position).ToListAsync();
                ItemsCollection = new ObservableList<OpenAIChat, Guid>(chats, p => p.Id);
            }

            await base.LoadStateAsync(destinationPageType, parameters, state, mode);
        }

        public override void SaveState(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            base.SaveState(destinationPageType, parameters, state, mode);

            state.SetValue(nameof(ChatType), ChatType);
        }

        #endregion

        #region Private Methods

        private async Task<IReadOnlyCollection<string>> GetSupportedChatModels(OpenAIChat chat)
        {
            try
            {
                var authentication = new OpenAIAuthentication(ApplicationSettings.Instance.OpenAIApiKey, ApplicationSettings.Instance.OpenAIOrganization);
                var api = new OpenAIClient(authentication);
                var allModels = await api.WrapAction((client) => client.ModelsEndpoint.GetModelsAsync());

                return chat.Type switch
                {
                    OpenAIChatType.Chat => allModels.Where(p => p.Id.Contains("gpt")).OrderByDescending(p => p.CreatedAt).Select(p => p.Id).ToList().AsReadOnly(),
                    OpenAIChatType.Image => allModels.Where(p => p.Id.Contains("dall")).OrderByDescending(p => p.CreatedAt).Select(p => p.Id).ToList().AsReadOnly(),
                    _ => null,
                };
            }
            catch (OpenAiException ex)
            {
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    await Window.CreateErrorDialog(ex).ShowAsync();
                });
            }
            catch (Exception ex)
            {
                ex.LogError();

                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    await Window.CreateExceptionDialog(ex).ShowAsync();
                });
            }

            return null;
        }

        #endregion
    }
}
