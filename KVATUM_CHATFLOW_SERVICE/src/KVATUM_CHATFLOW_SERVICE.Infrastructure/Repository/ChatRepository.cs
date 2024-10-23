using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ServerFlowDbContext _context;

        public ChatRepository(ServerFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Chat?> AddChatAsync(string name, ChatType type, Workspace workspace)
        {
            var chat = new Chat
            {
                Name = name,
                Type = type.ToString(),
            };

            workspace.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return chat;
        }

        public async Task<Chat?> AttachChatToWorkspaceAsync(Guid chatId, Workspace workspace)
        {
            var chat = await _context.Chats.Include(e => e.Workspaces)
                                           .FirstOrDefaultAsync(e => e.Id == chatId);
            if (chat == null)
                return null;

            if (chat.Workspaces.Any(e => e.Id == workspace.Id))
                return chat;

            workspace.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return chat;

        }

        public async Task<bool> DeleteChatAsync(Guid chatId)
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(e => e.Id == chatId);
            if (chat == null)
                return true;

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
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

        public async Task<Chat?> GetChatAsync(Guid chatId)
        {
            return await _context.Chats.FirstOrDefaultAsync(e => e.Id == chatId);
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
    }
}