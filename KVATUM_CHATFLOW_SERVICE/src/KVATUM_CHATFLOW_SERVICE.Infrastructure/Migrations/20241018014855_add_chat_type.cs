using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_chat_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Chats",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Chats");
        }
    }
}
