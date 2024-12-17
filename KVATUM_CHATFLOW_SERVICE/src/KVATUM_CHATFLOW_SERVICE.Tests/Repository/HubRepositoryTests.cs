using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository;
using KVATUM_CHATFLOW_SERVICE.Tests.Mocks.Data;
using Moq;
using Xunit.Abstractions;

namespace KVATUM_CHATFLOW_SERVICE.Tests.Repository
{
    public class HubRepositoryTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ServerFlowDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly IHubRepository _hubRepository;
        private readonly Guid _hubId = Guid.Parse("31f11688-d858-4275-b013-243b662f5d1b");

        public HubRepositoryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _cacheService = new Mock<ICacheService>().Object;
            _context = ChatflowDbFactory.Create();

            SetupCacheService();
            _hubRepository = new HubRepository(_context, _cacheService);
        }

        void SetupCacheService()
        {
            var cacheService = new Mock<ICacheService>();
            cacheService.Setup(e => e.GetAsync<CachedHub>($"hub:{It.IsAny<Guid>()}")).ReturnsAsync(new CachedHub
            {
                Id = _hubId,
                Name = "Test Hub",
                HexColor = "#000000",
            });
        }

        [Theory]
        [InlineData("31f11688-d858-4275-b013-243b662f5d1b", true)]
        [InlineData("00000000-0000-0000-0000-000000000001", false)]
        public async Task GetHubByIdAsync_ShouldReturnHub_WhenHubExists(Guid hubId, bool isSuccess)
        {
            var hub = await _hubRepository.GetHubByIdAsync(hubId);
            Assert.Equal(isSuccess, hub != null);
        }

        [Theory]
        [InlineData("123", true)]
        [InlineData("00000000-0000-0000-0000-000000000001", false)]
        public async Task GetLinkByHashAsync_ShouldReturnLink_WhenLinkExists(string hash, bool isSuccess)
        {
            var link = await _hubRepository.GetHubJoiningInvitationByHashAsync(hash);
            Assert.Equal(isSuccess, link != null);
        }

        [Theory]
        [InlineData("31f11688-d858-4275-b013-243b662f5d1b", true)]
        [InlineData("00000000-0000-0000-0000-000000000001", false)]
        public async Task GetLinkByHubIdAsync_ShouldReturnLink_WhenLinkExists(Guid hubId, bool isSuccess)
        {
            var link = await _hubRepository.GetHubJoiningInvitationByHubIdAsync(hubId);
            Assert.Equal(isSuccess, link != null);
        }

        [Theory]
        [InlineData("31f11688-d858-4275-b013-243b662f5d1b", "Test Hub 2", true)]
        [InlineData("00000000-0000-0000-0000-000000000001", "Test Hub 2", false)]
        public async Task UpdateHubNameAsync_ShouldReturnHub_WhenHubExists(Guid hubId, string name, bool isSuccess)
        {
            var hub = await _hubRepository.UpdateHubNameAsync(hubId, name);
            Assert.Equal(isSuccess, hub != null);
            if (isSuccess)
                Assert.Equal(name, hub.Name);
        }
    }
}