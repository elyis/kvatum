using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository;
using KVATUM_CHATFLOW_SERVICE.Tests.Mocks.Data;
using Moq;
using Xunit.Abstractions;

namespace KVATUM_CHATFLOW_SERVICE.Tests.Repository
{
    public class WorkspaceRepositoryTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ServerFlowDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly Workspace _workspace;

        public WorkspaceRepositoryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _cacheService = new Mock<ICacheService>().Object;
            _context = ChatflowDbFactory.Create();

            _workspace = _context.Workspaces.First();
            SetupCacheService(_workspace);
            _workspaceRepository = new WorkspaceRepository(_context, _cacheService);
        }

        void SetupCacheService(Workspace workspace)
        {
            var cacheService = new Mock<ICacheService>();
            cacheService.Setup(e => e.GetAsync<CachedWorkspace>($"workspace:{workspace.Id}")).ReturnsAsync(workspace.ToCachedWorkspace());
        }

        [Theory]
        [InlineData("31f11688-d858-4275-b013-243b662f5d1b", true)]
        [InlineData("00000000-0000-0000-0000-000000000001", false)]
        public async Task GetWorkspaceAsync_ShouldReturnWorkspace(Guid workspaceId, bool isSuccess)
        {
            var workspace = await _workspaceRepository.GetWorkspaceAsync(workspaceId);
            Assert.Equal(isSuccess, workspace != null);
        }

        [Theory]
        [InlineData("31f11688-d858-4275-b013-243b662f5d1b", true)]
        [InlineData("00000000-0000-0000-0000-000000000001", false)]
        public async Task GetWorkspaceAsync_ShouldExpectedResult(Guid workspaceId, bool isSuccess)
        {
            var workspace = await _workspaceRepository.GetWorkspaceAsync(workspaceId);
            Assert.Equal(isSuccess, workspace != null);
        }

        [Fact]
        public async Task AddWorkspaceAsync_ShouldReturnWorkspace()
        {
            var workspace = await _workspaceRepository.AddWorkspaceAsync("Test Workspace", _workspace.HubId, "#000000");
            Assert.NotNull(workspace);
        }

        [Theory]
        [InlineData("31f11688-d858-4275-b013-243b662f5d1b", true)]
        [InlineData("00000000-0000-0000-0000-000000000001", false)]
        public async Task UpdateWorkspaceIconAsync_ShouldReturnWorkspace(Guid workspaceId, bool isSuccess)
        {
            var workspace = await _workspaceRepository.UpdateWorkspaceIconAsync(workspaceId, "test.png");
            Assert.Equal(isSuccess, workspace != null);
        }
    }
}