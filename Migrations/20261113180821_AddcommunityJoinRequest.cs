using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace microblog_heads.Migrations
{
    /// <inheritdoc />
    public partial class AddcommunityJoinRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "community_join_request",
                schema: "Microblog",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    community_id = table.Column<int>(type: "integer", nullable: false),
                    regular_user_id = table.Column<int>(type: "integer", nullable: false),
                    date_requested = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("community_join_request_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_join_request_community",
                        column: x => x.community_id,
                        principalSchema: "Microblog",
                        principalTable: "community",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_join_request_user",
                        column: x => x.regular_user_id,
                        principalSchema: "Microblog",
                        principalTable: "regular_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_community_join_request_regular_user_id",
                schema: "Microblog",
                table: "community_join_request",
                column: "regular_user_id");

            migrationBuilder.CreateIndex(
                name: "uq_join_request",
                schema: "Microblog",
                table: "community_join_request",
                columns: new[] { "community_id", "regular_user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "community_join_request",
                schema: "Microblog");
        }
    }
}
