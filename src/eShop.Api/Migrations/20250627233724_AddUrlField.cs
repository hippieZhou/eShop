using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUrlField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "Blogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "url",
                table: "Blogs");
        }
    }
}
