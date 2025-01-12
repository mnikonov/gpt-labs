using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.ViewModels.Collections;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gpt.Labs.ViewModels;

public class DataManager
{
    #region Fields

    private static readonly object SyncRoot = new();

    private static volatile DataManager _instance;

    private readonly Dictionary<OpenAIChatType, ObservableList<OpenAIChat, Guid>> _data = [];

    #endregion

    #region Constructors

    private DataManager()
    {
        using (var context = new DataContext())
        {
            var chats = context.Chats.Include(p => p.Settings).AsNoTracking().GroupBy(p => p.Type).ToDictionary(p => p.Key, p => p.OrderBy(p => p.Position));

            _data[OpenAIChatType.Chat] = new ObservableList<OpenAIChat, Guid>(chats.ContainsKey(OpenAIChatType.Chat) ? chats[OpenAIChatType.Chat] : [], p => p.Id);
            _data[OpenAIChatType.Image] = new ObservableList<OpenAIChat, Guid>(chats.ContainsKey(OpenAIChatType.Image) ? chats[OpenAIChatType.Image] : [], p => p.Id);
        }
    }

    #endregion

    #region Properties

    public static DataManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (SyncRoot)
            {
                _instance ??= new DataManager();
            }

            return _instance;
        }
    }

    public ObservableList<OpenAIChat, Guid> this[OpenAIChatType index] => _data[index];

    #endregion

    #region Methods

    public OpenAIChat GetChat(Guid chatId)
    {
        foreach (var chats in _data.Values)
        {
            var chat = chats.GetById(chatId);

            if (chat != null)
            {
                return chat;
            }
        }

        throw new KeyNotFoundException($"The chat with Id '{chatId}' was not found.");

    }

    #endregion
}
