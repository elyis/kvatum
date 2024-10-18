using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Configurations
{
    public class HubConfiguration : IEntityTypeConfiguration<Hub>
    {
        public void Configure(EntityTypeBuilder<Hub> builder)
        {
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasMany(e => e.Workspaces)
                .WithOne(e => e.Hub)
                .HasForeignKey(e => e.HubId);
        }
    }
}