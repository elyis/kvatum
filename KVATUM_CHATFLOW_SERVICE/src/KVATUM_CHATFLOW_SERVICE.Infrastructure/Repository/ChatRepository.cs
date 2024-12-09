using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ServerFlowDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly string _cacheChatKeyPrefix = "chat:";

        public ChatRepository(
            ServerFlowDbContext context,
            ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<CachedChat?> AddChatAsync(string name, ChatType type, Guid workspaceId)
        {
            var chat = new Chat
            {
                Name = name,
                Type = type.ToString(),
            };

            var workspaceChat = new WorkspaceChat
            {
                WorkspaceId = workspaceId,
                ChatId = chat.Id,
            };

            await _context.WorkspaceChats.AddAsync(workspaceChat);
            await _context.SaveChangesAsync();
            await CacheChat(chat);
            return chat.ToCachedChat();
        }

        public async Task<Chat?> AttachChatToWorkspaceAsync(Guid chatId, Guid workspaceId)
        {
            var workspace = await _context.Workspaces.FirstOrDefaultAsync(e => e.Id == workspaceId);
            if (workspace == null)
                return null;

            var workspaceChat = await _context.WorkspaceChats.Include(e => e.Chat)
                                                            .FirstOrDefaultAsync(e => e.ChatId == chatId && e.WorkspaceId == workspaceId);
            if (workspaceChat != null)
                return workspaceChat.Chat;

            workspaceChat = new WorkspaceChat
            {
                WorkspaceId = workspaceId,
                ChatId = chatId,
            };

            _context.WorkspaceChats.Add(workspaceChat);
            await _context.SaveChangesAsync();
            return workspaceChat.Chat;
        }

        public async Task<bool> DeleteChatAsync(Guid chatId)
        {
            var cachedChat = await _cacheService.GetAsync<CachedChat>($"{_cacheChatKeyPrefix}{chatId}");
            var chat = new Chat { Id = chatId };
            if (cachedChat == null)
            {
                chat = await _context.Chats.FirstOrDefaultAsync(e => e.Id == chatId);
                if (chat == null)
                    return true;
            }

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
            await RemoveCachedChat(chatId);
            return true;
        }

        public async Task<bool> DetachChatFromWorkspaceAsync(Guid chatId, Guid workspaceId)
        {
            var chat = await _context.Chats.Include(e => e.Workspaces)
                                           .FirstOrDefaultAsync(e => e.Id == chatId);
            if (chat == null)
                return true;

            if (!chat.Workspaces.Any(e => e.Id == workspaceId))
                return true;

            chat.Workspaces.Remove(chat.Workspaces.First(e => e.Id == workspaceId));
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CachedChat?> GetChatAsync(Guid chatId)
        {
            var cachedChat = await _cacheService.GetAsync<CachedChat>($"{_cacheChatKeyPrefix}{chatId}");
            if (cachedChat != null)
                return cachedChat;

            var chat = await _context.Chats.FirstOrDefaultAsync(e => e.Id == chatId);
            if (chat == null)
                return null;

            await CacheChat(chat);
            return chat.ToCachedChat();
        }

        public async Task<List<Chat>> GetChatsByWorkspaceIdAsync(Guid workspaceId)
        {
            var workspace = await _context.Workspaces.Include(e => e.Chats)
                                                     .FirstOrDefaultAsync(e => e.Id == workspaceId);
            return workspace?.Chats ?? new List<Chat>();
        }

        public async Task<List<WorkspaceChatsBody>> GetWorkspaceChatsAsync(IEnumerable<Guid> workspaceIds)
        {
            var workspaces = await _context.Workspaces.Where(e => workspaceIds.Contains(e.Id))
                                                      .Include(e => e.Chats)
                                                      .ToListAsync();
            return workspaces.Select(e => new WorkspaceChatsBody
            {
                WorkspaceId = e.Id,
                Chats = e.Chats.Select(e => e.ToChatBody()).ToList()
            }).ToList();
        }

        private async Task CacheChat(Chat chat)
        {
            await _cacheService.SetAsync($"{_cacheChatKeyPrefix}{chat.Id}", chat.ToCachedChat(), TimeSpan.FromHours(1), TimeSpan.FromHours(6));
        }

        private async Task RemoveCachedChat(Guid chatId)
        {
            await _cacheService.RemoveAsync($"{_cacheChatKeyPrefix}{chatId}");
        }
    }
}