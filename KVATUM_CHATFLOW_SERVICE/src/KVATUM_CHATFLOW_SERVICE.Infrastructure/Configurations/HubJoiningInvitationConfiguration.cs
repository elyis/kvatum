using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Configurations
{
    public class HubJoiningInvitationConfiguration : IEntityTypeConfiguration<HubJoiningInvitation>
    {
        public void Configure(EntityTypeBuilder<HubJoiningInvitation> builder)
        {
            builder.HasKey(e => new { e.HubId, e.HashInvitation });
            builder.HasOne(e => e.Hub)
                   .WithOne(e => e.HubJoiningInvitation)
                   .HasForeignKey<HubJoiningInvitation>(e => e.HubId);
        }
    }
}