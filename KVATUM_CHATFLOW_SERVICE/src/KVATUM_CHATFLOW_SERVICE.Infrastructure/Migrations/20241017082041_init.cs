using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HubJoiningInvitations",
                columns: table => new
                {
                    HubId = table.Column<Guid>(type: "uuid", nullable: false),
                    HashInvitation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HubJoiningInvitations", x => new { x.HubId, x.HashInvitation });
                    table.ForeignKey(
                        name: "FK_HubJoiningInvitations_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HubMembers",
                columns: table => new
                {
                    HubId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HubMembers", x => new { x.HubId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_HubMembers_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HubId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workspaces_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatWorkspace",
                columns: table => new
                {
                    ChatsId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspacesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatWorkspace", x => new { x.ChatsId, x.WorkspacesId });
                    table.ForeignKey(
                        name: "FK_ChatWorkspace_Chats_ChatsId",
                        column: x => x.ChatsId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatWorkspace_Workspaces_WorkspacesId",
                        column: x => x.WorkspacesId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatWorkspace_WorkspacesId",
                table: "ChatWorkspace",
                column: "WorkspacesId");

            migrationBuilder.CreateIndex(
                name: "IX_HubJoiningInvitations_HubId",
                table: "HubJoiningInvitations",
                column: "HubId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_HubId",
                table: "Workspaces",
                column: "HubId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatWorkspace");

            migrationBuilder.DropTable(
                name: "HubJoiningInvitations");

            migrationBuilder.DropTable(
                name: "HubMembers");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropTable(
                name: "Hubs");
        }
    }
}
