using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnetAPI.Migrations
{
    /// <inheritdoc />
    public partial class updating6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryType",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryType",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
