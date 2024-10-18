using System.Reflection;
using Microsoft.EntityFrameworkCore;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Data
{
    public class ServerFlowDbContext : DbContext
    {
        public ServerFlowDbContext(DbContextOptions<ServerFlowDbContext> options) : base(options)
        {
        }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Hub> Hubs { get; set; }
        public DbSet<HubMember> HubMembers { get; set; }
        public DbSet<HubJoiningInvitation> HubJoiningInvitations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}