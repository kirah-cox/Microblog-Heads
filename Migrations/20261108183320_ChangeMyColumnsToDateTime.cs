using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace microblog_heads.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMyColumnsToDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "date_posted",
                schema: "Microblog",
                table: "post",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_posted",
                schema: "Microblog",
                table: "comment",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_posted",
                schema: "Microblog",
                table: "comment");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "date_posted",
                schema: "Microblog",
                table: "post",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
