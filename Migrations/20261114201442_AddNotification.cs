using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace microblog_heads.Migrations
{
    /// <inheritdoc />
    public partial class AddNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification",
                schema: "Microblog",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notifier_user_id = table.Column<int>(type: "integer", nullable: false),
                    notified_user_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    post_id = table.Column<int>(type: "integer", nullable: false),
                    comment_id = table.Column<int>(type: "integer", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    date_created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notification_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_comment",
                        column: x => x.comment_id,
                        principalSchema: "Microblog",
                        principalTable: "comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notification_notified",
                        column: x => x.notified_user_id,
                        principalSchema: "Microblog",
                        principalTable: "regular_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notification_notifier",
                        column: x => x.notifier_user_id,
                        principalSchema: "Microblog",
                        principalTable: "regular_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notification_post",
                        column: x => x.post_id,
                        principalSchema: "Microblog",
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_comment_id",
                schema: "Microblog",
                table: "notification",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_notified_is_read",
                schema: "Microblog",
                table: "notification",
                columns: new[] { "notified_user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_notifier_user_id",
                schema: "Microblog",
                table: "notification",
                column: "notifier_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_post_id",
                schema: "Microblog",
                table: "notification",
                column: "post_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification",
                schema: "Microblog");
        }
    }
}
