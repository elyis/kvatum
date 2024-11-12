using System.Net;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_CHATFLOW_SERVICE.App.Service
{
    public class HubService : IHubService
    {
        private readonly IHubRepository _hubRepository;
        private readonly IHashGenerator _hashGenerator;
        private readonly ILogger<HubService> _logger;
        public HubService(
            IHubRepository hubRepository,
            IHashGenerator hashGenerator,
            ILogger<HubService> logger)
        {
            _hubRepository = hubRepository;
            _hashGenerator = hashGenerator;
            _logger = logger;
        }

        public async Task<ServiceResponse<bool>> AddMemberToHubAsync(Guid memberId, string hash)
        {
            var hubInvitation = await _hubRepository.GetHubJoiningInvitationByHashAsync(hash);
            if (hubInvitation == null)
                return new ServiceResponse<bool>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Joining invitation not found" }
                };

            await _hubRepository.AddMemberToHubAsync(hubInvitation.HubId, memberId);

            return new ServiceResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = true
            };
        }

        public async Task<ServiceResponse<HubBody>> CreateHubAsync(CreateHubBody body, Guid creatorId)
        {
            var hashInvitation = _hashGenerator.Compute();
            string hexColor = RandomColorGenerator.GetRandomColor();
            var hub = await _hubRepository.AddHubAsync(body, creatorId, hashInvitation, hexColor);

            if (hub == null)
            {
                return new ServiceResponse<HubBody>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Errors = new[] { "Hub not created" }
                };
            }

            return new ServiceResponse<HubBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = hub?.ToHubBody()
            };
        }

        public async Task<ServiceResponse<List<HubBody>>> GetConnectedHubsAsync(Guid connectedAccountId, int limit, int offset)
        {
            var errors = new List<string>();
            if (limit <= 0)
                errors.Add("Limit is not valid");
            if (offset < 0)
                errors.Add("Offset is not valid");

            if (errors.Any())
            {
                return new ServiceResponse<List<HubBody>>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Errors = errors.ToArray()
                };
            }

            var hubs = await _hubRepository.GetHubsByConnectedAccountIdAsync(connectedAccountId, limit, offset);
            return new ServiceResponse<List<HubBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = hubs.Select(e => e.ToHubBody()).ToList()
            };
        }

        public async Task<ServiceResponse<HubInvitationLinkBody>> GetHubJoiningInvitationByHubIdAsync(Guid hubId)
        {
            var joiningInvitation = await _hubRepository.GetHubJoiningInvitationByHubIdAsync(hubId);
            if (joiningInvitation == null)
                return new ServiceResponse<HubInvitationLinkBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Joining invitation not found" }
                };

            return new ServiceResponse<HubInvitationLinkBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = new HubInvitationLinkBody { Link = joiningInvitation.HashInvitation }
            };
        }
    }
}