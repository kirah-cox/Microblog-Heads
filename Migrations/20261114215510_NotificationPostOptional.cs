using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace microblog_heads.Migrations
{
    /// <inheritdoc />
    public partial class NotificationPostOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "post_id",
                schema: "Microblog",
                table: "notification",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "post_id",
                schema: "Microblog",
                table: "notification",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
