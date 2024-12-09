using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Configurations
{
    public class WorkspaceChatConfiguration : IEntityTypeConfiguration<WorkspaceChat>
    {
        public void Configure(EntityTypeBuilder<WorkspaceChat> builder)
        {
            builder.HasKey(e => new { e.WorkspaceId, e.ChatId });
        }
    }
}