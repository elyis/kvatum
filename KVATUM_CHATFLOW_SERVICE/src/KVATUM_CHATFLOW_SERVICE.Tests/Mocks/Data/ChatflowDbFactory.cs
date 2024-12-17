using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KVATUM_CHATFLOW_SERVICE.Tests.Mocks.Data
{
    public class ChatflowDbFactory
    {
        public static ServerFlowDbContext Create(string? name = null)
        {
            var options = new DbContextOptionsBuilder<ServerFlowDbContext>()
                .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
                .Options;

            var context = new ServerFlowDbContext(options);

            var hubs = GenerateHubs();
            var workspaces = GenerateWorkspaces(hubs);
            var chats = GenerateChats(workspaces);

            context.Hubs.AddRange(hubs);
            context.Workspaces.AddRange(workspaces);
            context.Chats.AddRange(chats);

            context.SaveChanges();
            return context;
        }

        private static IEnumerable<Hub> GenerateHubs()
        {
            return new List<Hub>
            {
                new() {
                    Id = Guid.Parse("31f11688-d858-4275-b013-243b662f5d1b"),
                    Name = "Test Hub",
                    HexColor = "#000000",
                    HubJoiningInvitation = new HubJoiningInvitation
                    {
                        HashInvitation = "123",
                    },
                },

                new() {
                    Id = Guid.NewGuid(),
                    Name = "Test Hub",
                    HexColor = "#000000",
                    HubJoiningInvitation = new HubJoiningInvitation
                    {
                        HashInvitation = "123",
                    },
                },

                new() {
                    Id = Guid.NewGuid(),
                    Name = "Test Hub2",
                    HexColor = "#000000",
                    HubJoiningInvitation = new HubJoiningInvitation
                    {
                        HashInvitation = "123",
                    },
                },

                new() {
                    Id = Guid.NewGuid(),
                    Name = "Test Hub3",
                    HexColor = "#000000",
                    HubJoiningInvitation = new HubJoiningInvitation
                    {
                        HashInvitation = "123",
                    },
                },

                new() {
                    Id = Guid.NewGuid(),
                    Name = "Test Hub4",
                    HexColor = "#000000",
                    HubJoiningInvitation = new HubJoiningInvitation
                    {
                        HashInvitation = "123",
                    },
                },

                new() {
                    Id = Guid.NewGuid(),
                    Name = "Test Hub5",
                    HexColor = "#000000",
                    HubJoiningInvitation = new HubJoiningInvitation
                    {
                        HashInvitation = "123",
                    },
                },

                new() {
                    Id = Guid.NewGuid(),
                    Name = "Test Hub6",
                    HexColor = "#000000",
                    HubJoiningInvitation = new HubJoiningInvitation
                    {
                        HashInvitation = "123",
                    },
                },
            };
        }

        private static IEnumerable<Workspace> GenerateWorkspaces(IEnumerable<Hub> hubs)
        {
            var workspaces = new List<Workspace>();
            foreach (var element in hubs)
            {
                workspaces.Add(new Workspace
                {
                    Id = Guid.NewGuid(),
                    Name = $"test",
                    Hub = element,
                    HexColor = "#000000",
                });
            }

            workspaces.Add(new Workspace
            {
                Id = Guid.Parse("31f11688-d858-4275-b013-243b662f5d1b"),
                Name = $"test",
                Hub = hubs.First(),
                HexColor = "#000000",
            });

            return workspaces;
        }

        private static IEnumerable<Chat> GenerateChats(IEnumerable<Workspace> workspaces)
        {
            var chats = new List<Chat>();
            foreach (var workspace in workspaces)
            {
                var chat = new Chat
                {
                    Id = Guid.NewGuid(),
                    Name = $"test",
                    Type = ChatType.Chat.ToString(),
                };

                chats.Add(chat);
                workspace.Chats.Add(chat);
            }

            chats.Add(new Chat
            {
                Id = Guid.Parse("31f11688-d858-4275-b013-243b662f5d1b"),
                Name = $"test",
                Type = ChatType.Chat.ToString(),
            });

            return chats;
        }
    }
}
