using Gpt.Labs.Models.Enums;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gpt.Labs.Models.Extensions
{
    public static class OpenAIMessageExtensions
    {
        #region Fields

        private static Func<DataContext, Guid, Guid, bool> ContainsFunc { get; } = EF.CompileQuery(
            (DataContext context, Guid chatId, Guid id) =>
            context.Messages.AsNoTracking().Any(p => p.ChatId == chatId && p.Id == id));

        private static Func<DataContext, Guid, Guid, OpenAIMessage> GetByIdFunc { get; } = EF.CompileQuery(
            (DataContext context, Guid chatId, Guid id) =>
            context.Messages.AsNoTracking().FirstOrDefault(p => p.ChatId == chatId && p.Id == id));

        private static Func<DataContext, Guid, int> CountFunc { get; } = EF.CompileQuery(
            (DataContext context, Guid chatId) =>
            context.Messages.AsNoTracking().Count(p => p.ChatId == chatId));

        private static Func<DataContext, Guid, int, int, IEnumerable<OpenAIMessage>> GetInRangeFunc { get; } = EF.CompileQuery(
            (DataContext context, Guid chatId, int skip, int take) =>
            context.Messages.AsNoTracking().Where(p => p.ChatId == chatId).OrderBy(p => p.CreatedDate).Skip(skip).Take(take));

        public static Func<DataContext, Guid, DateTime, int> IndexOfFunc { get; } = EF.CompileQuery(
            (DataContext context, Guid chatId, DateTime maxDateAdded) =>
            context.Messages.AsNoTracking().Count(p => p.ChatId == chatId && p.CreatedDate <= maxDateAdded));

        #endregion

        #region Public Methods

        public static Message ToChatRequestMessage(this OpenAIMessage message)
        {
            Role role = Role.User;
            switch (message.Role)
            {
                case Enums.OpenAIRole.System:
                    role = Role.System;
                    break;
                case Enums.OpenAIRole.Assistant:
                    role = Role.Assistant;
                    break;
                case Enums.OpenAIRole.User:
                    role = Role.User;
                    break;
            }
            return new Message(role, message.Content);
        }

        public static bool Contains(
            this DbSet<OpenAIMessage> set,
            Guid chatId,
            Guid id)
        {
            return ContainsFunc(set.GetDataContext(), chatId, id);
        }

        public static OpenAIMessage GetById(
            this DbSet<OpenAIMessage> set,
            Guid chatId,
            Guid id)
        {
            return GetByIdFunc(set.GetDataContext(), chatId, id);
        }

        public static int Count(
            this DbSet<OpenAIMessage> set,
            Guid chatId)
        {
            return CountFunc(set.GetDataContext(), chatId);
        }

        public static IEnumerable<OpenAIMessage> GetInRange(
            this DbSet<OpenAIMessage> set,
            Guid chatId,
            int skip,
            int take)
        {
            return GetInRangeFunc(set.GetDataContext(), chatId, skip, take);
        }

        public static int IndexOf(
            this DbSet<OpenAIMessage> set,
            OpenAIMessage message)
        {
            return IndexOfFunc(set.GetDataContext(), message.ChatId, message.CreatedDate);
        }

        #endregion
    }
}
