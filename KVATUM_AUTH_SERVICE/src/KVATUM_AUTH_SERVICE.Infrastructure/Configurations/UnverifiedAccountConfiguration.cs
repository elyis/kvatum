using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Configurations
{
    public class UnverifiedAccountConfiguration : IEntityTypeConfiguration<UnverifiedAccount>
    {
        public void Configure(EntityTypeBuilder<UnverifiedAccount> builder)
        {
            builder.HasKey(u => u.Id);

            builder.HasIndex(u => u.Email).IsUnique();
        }

    }
}