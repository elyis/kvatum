using System.Reflection;
using Microsoft.EntityFrameworkCore;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountSession> AccountSessions { get; set; }
        public DbSet<UnverifiedAccount> UnverifiedAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}