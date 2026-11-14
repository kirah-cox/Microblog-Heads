using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace microblog_heads.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityIsPrivate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_private",
                schema: "Microblog",
                table: "community",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_private",
                schema: "Microblog",
                table: "community");
        }
    }
}
