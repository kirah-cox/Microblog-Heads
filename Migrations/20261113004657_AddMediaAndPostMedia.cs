using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace microblog_heads.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAndPostMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media",
                schema: "Microblog",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("media_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_media",
                schema: "Microblog",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    post_id = table.Column<int>(type: "integer", nullable: false),
                    media_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("post_media_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_media_media",
                        column: x => x.media_id,
                        principalSchema: "Microblog",
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_media_post",
                        column: x => x.post_id,
                        principalSchema: "Microblog",
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_post_media_media_id",
                schema: "Microblog",
                table: "post_media",
                column: "media_id");

            migrationBuilder.CreateIndex(
                name: "uq_post_media",
                schema: "Microblog",
                table: "post_media",
                columns: new[] { "post_id", "media_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_media",
                schema: "Microblog");

            migrationBuilder.DropTable(
                name: "media",
                schema: "Microblog");
        }
    }
}
