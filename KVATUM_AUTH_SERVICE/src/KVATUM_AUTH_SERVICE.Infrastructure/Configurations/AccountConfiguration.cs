using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(256);

            builder.Property(a => a.Nickname)
                .IsRequired();

            builder.Property(a => a.Role)
                .IsRequired();

            builder.Property(a => a.PasswordHash)
                .IsRequired();

            builder.HasMany(a => a.Sessions)
                .WithOne(s => s.Account)
                .HasForeignKey(s => s.AccountId);
        }
    }
}