using Gpt.Labs.Controls.Dialogs;
using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.ViewModels.Base;
using Gpt.Labs.ViewModels.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OpenAI.Chat;
using System;
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

        public ChatsListViewModel()
        {
        }

        #endregion

        #region Properties

        public OpenAIChatType ChatType { get; private set; }

        public bool MultiSelectModeEnabled 
        { 
            get => this.multiSelectModeEnabled; 
            set => this.Set(ref this.multiSelectModeEnabled, value); 
        }

        #endregion

        #region Public Methods

        public async Task<bool> AddEditChat(OpenAIChat chat)
        {
            var dialogModel = new OpenAIChat() { Title = chat?.Title, Type = this.ChatType, Position = 0 };

            OpenAISettings settings = null;

            switch (ChatType)
            {
                case OpenAIChatType.Chat:
                    var chatSettings = new OpenAIChatSettings() { Type = this.ChatType };
                    chatSettings.SystemMessage = ((OpenAIChatSettings)chat?.Settings)?.SystemMessage;
                    chatSettings.ModelId = ((OpenAIChatSettings)chat?.Settings)?.ModelId;
                    settings = chatSettings;
                    break;
                case OpenAIChatType.Image:
                    var imageSettings = new OpenAIImageSettings() { Type = this.ChatType };
                    imageSettings.Size = ((OpenAIImageSettings)chat?.Settings)?.Size ?? OpenAIImageSize.Large;
                    settings = imageSettings;
                    break;
            }

            dialogModel.Settings = settings;

            ContentDialogBase dialog = null;

            switch (ChatType)
            {
                case OpenAIChatType.Chat:
                    dialog = new EditChatDialog(dialogModel);
                    break;
                case OpenAIChatType.Image:
                    dialog = new EditImageDialog(dialogModel);
                    break;
            }

            var result = await dialog.ShowAsync();

            var edited = false;

            if (result == ContentDialogResult.Primary)
            {
                if (chat != null)
                {
                    chat.Title = dialogModel.Title;

                    switch (ChatType)
                    {
                        case OpenAIChatType.Chat:
                            ((OpenAIChatSettings)chat.Settings).SystemMessage = ((OpenAIChatSettings)settings).SystemMessage;
                            ((OpenAIChatSettings)chat.Settings).ModelId = ((OpenAIChatSettings)settings).ModelId;
                            break;
                        case OpenAIChatType.Image:
                            ((OpenAIImageSettings)chat.Settings).Size = ((OpenAIImageSettings)settings).Size;
                            break;
                    }
                }

                using (var context = new DataContext())
                {
                    if (chat != null)
                    {
                        context.Entry(chat).State = EntityState.Modified;
                        context.Entry(chat.Settings).State = EntityState.Modified;

                        edited = true;
                    }
                    else
                    {
                        for (int i = 0; i < this.ItemsCollection.Count; i++)
                        {
                            var item = this.ItemsCollection[i];
                            item.Position = i + 1;
                            context.Entry(item).State = EntityState.Modified;
                        }

                        context.Add(dialogModel);
                    }

                    await context.SaveChangesAsync();
                }

                if (chat == null)
                {
                    this.ItemsCollection.Insert(0, dialogModel);
                }
            }

            return edited;
        }

        public async Task DeleteChats(params OpenAIChat[] chats)
        {
            ContentDialog dialog;

            if (chats.Length == 1)
            {
                dialog = "Confirm".CreateYesNoDialog("DeleteChat", chats[0].Title);
            }
            else
            {
                dialog = "Confirm".CreateYesNoDialog("DeleteChats");
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                using (var context = new DataContext())
                {
                    foreach (var chat in chats)
                    {
                        for (int i = chat.Position + 1; i < this.ItemsCollection.Count; i++)
                        {
                            var item = this.ItemsCollection[i];
                            item.Position = i - 1;
                            context.Entry(item).State = EntityState.Modified;
                        }
                    
                        context.Entry(chat).State = EntityState.Deleted;

                        this.ItemsCollection.Remove(chat);

                        if (this.SelectedElement != null && chat.Id == this.SelectedElement.Id)
                        {
                            this.SelectedElement = null;
                        }
                    }  

                    await context.SaveChangesAsync();
                }

                foreach (var chat in chats)
                {
                    if (chat.Type == OpenAIChatType.Image)
                    {
                        var chatFolder = (await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(chat.Id.ToString()));

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
            using (var context = new DataContext())
            {
                var newIndex = this.ItemsCollection.IndexOf(chat);

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
                    var item = this.ItemsCollection[i];
                    item.Position = i;
                    context.Entry(item).State = EntityState.Modified;
                }

                await context.SaveChangesAsync();
            }
        }

        public void SelectChat(OpenAIChat chat)
        {
            this.SelectedElement = chat;
        }

        public override async Task LoadStateAsync(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            if (mode == NavigationMode.New)
            {
                this.ChatType = parameters.GetValue<OpenAIChatType>("chat-type");
            }
            else
            {
                this.ChatType = state.GetValue<OpenAIChatType>(nameof(this.ChatType));
            }

            using (var context = new DataContext())
            {
                var chats = await context.Chats.Include(p => p.Settings).AsNoTracking().Where(p => p.Type == this.ChatType).OrderBy(p => p.Position).ToListAsync();
                this.ItemsCollection = new ObservableList<OpenAIChat, Guid>(chats, p => p.Id);
            }
            
            await base.LoadStateAsync(destinationPageType, parameters, state, mode);
        }

        public override void SaveState(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            base.SaveState(destinationPageType, parameters, state, mode);

            state.SetValue(nameof(this.ChatType), this.ChatType);
        }

        #endregion
    }
}
