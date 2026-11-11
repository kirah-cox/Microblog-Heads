using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace microblog_heads.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_role_id",
                schema: "Microblog",
                table: "regular_user",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "Microblog",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_role_pkey", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "Microblog",
                table: "user_role",
                columns: new[] { "id", "role" },
                values: new object[,]
                {
                    { 1, "User" },
                    { 2, "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_regular_user_user_role_id",
                schema: "Microblog",
                table: "regular_user",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "uq_user_role_role",
                schema: "Microblog",
                table: "user_role",
                column: "role",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_regular_user_user_role",
                schema: "Microblog",
                table: "regular_user",
                column: "user_role_id",
                principalSchema: "Microblog",
                principalTable: "user_role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_regular_user_user_role",
                schema: "Microblog",
                table: "regular_user");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "Microblog");

            migrationBuilder.DropIndex(
                name: "IX_regular_user_user_role_id",
                schema: "Microblog",
                table: "regular_user");

            migrationBuilder.DropColumn(
                name: "user_role_id",
                schema: "Microblog",
                table: "regular_user");
        }
    }
}
