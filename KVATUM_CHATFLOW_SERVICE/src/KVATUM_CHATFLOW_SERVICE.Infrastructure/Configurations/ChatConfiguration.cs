using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasMany(e => e.Workspaces).WithMany(e => e.Chats);
        }

    }
}