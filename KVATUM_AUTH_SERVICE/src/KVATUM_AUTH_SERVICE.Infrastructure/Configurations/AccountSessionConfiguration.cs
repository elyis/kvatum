using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Configurations
{
    public class AccountSessionConfiguration : IEntityTypeConfiguration<AccountSession>
    {
        public void Configure(EntityTypeBuilder<AccountSession> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Account)
                .WithMany(a => a.Sessions)
                .HasForeignKey(s => s.AccountId);
        }
    }
}