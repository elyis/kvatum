using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class table_WorkspaceChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkspaceChats",
                columns: table => new
                {
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceChats", x => new { x.WorkspaceId, x.ChatId });
                    table.ForeignKey(
                        name: "FK_WorkspaceChats_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceChats_ChatId",
                table: "WorkspaceChats",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceChats");
        }
    }
}
