using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class workspace_icon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HexColor",
                table: "Workspaces",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Workspaces",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HexColor",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Workspaces");
        }
    }
}
