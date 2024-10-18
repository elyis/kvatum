using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Configurations
{
    public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasOne(e => e.Hub)
                   .WithMany(e => e.Workspaces);

            builder.HasMany(e => e.Chats).WithMany(e => e.Workspaces);
        }
    }
}