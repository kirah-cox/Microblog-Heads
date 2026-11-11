using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace microblog_heads.Migrations
{
    /// <summary>
    /// Makes regular_user.username and regular_user.email unique, ignoring case
    /// ("Kirah" and "kirah" count as the same username).
    /// EF can't model an index on lower(column), so this uses raw SQL. IF NOT EXISTS
    /// keeps it safe on databases where these indexes were already created by hand.
    /// </summary>
    public partial class UniqueUsernameAndEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS uq_regular_user_username_ci
                    ON "Microblog".regular_user (lower(username));
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS uq_regular_user_email_ci
                    ON "Microblog".regular_user (lower(email));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "Microblog".uq_regular_user_username_ci;""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "Microblog".uq_regular_user_email_ci;""");
        }
    }
}
