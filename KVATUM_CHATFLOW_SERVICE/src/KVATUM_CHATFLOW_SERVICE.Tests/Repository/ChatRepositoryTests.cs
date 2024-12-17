using KVATUM_CHATFLOW_SERVICE.Core.Enums;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository;
using KVATUM_CHATFLOW_SERVICE.Tests.Mocks.Data;
using Moq;
using Xunit.Abstractions;

namespace KVATUM_CHATFLOW_SERVICE.Tests.Repository
{
    public class ChatRepositoryTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IChatRepository _chatRepository;
        private readonly ServerFlowDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly Guid _workspaceId = Guid.Parse("31f11688-d858-4275-b013-243b662f5d1b");

        public ChatRepositoryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _context = ChatflowDbFactory.Create();
            _cacheService = new Mock<ICacheService>().Object;
            _chatRepository = new ChatRepository(_context, _cacheService);
        }

        [Theory]
        [InlineData("31f11688-d858-4275-b013-243b662f5d1b", true)]
        [InlineData("00000000-0000-0000-0000-000000000001", false)]
        public async Task GetChatAsync_ShouldExpectedResult(Guid chatId, bool isSuccess)
        {
            var chat = await _chatRepository.GetChatAsync(chatId);
            Assert.Equal(isSuccess, chat != null);
        }

        [Fact]
        public async Task AddChatAsync_ShouldReturnChat()
        {
            var chat = await _chatRepository.AddChatAsync("Test Chat", ChatType.Chat, _workspaceId);
            Assert.NotNull(chat);
        }

        [Theory]
        [InlineData("31f11688-d858-4275-b013-243b662f5d1b", 1)]
        [InlineData("00000000-0000-0000-0000-000000000001", 0)]
        public async Task GetChatsByWorkspaceIdAsync_ShouldReturnChats(Guid workspaceId, int expectedCount)
        {
            var chats = await _chatRepository.GetChatsByWorkspaceIdAsync(workspaceId);
            Assert.Equal(expectedCount, chats.Count);
        }
    }
}