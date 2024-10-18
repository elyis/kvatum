using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Configurations
{
    public class HubMemberConfiguration : IEntityTypeConfiguration<HubMember>
    {
        public void Configure(EntityTypeBuilder<HubMember> builder)
        {
            builder.HasKey(e => new { e.HubId, e.MemberId });
        }
    }
}