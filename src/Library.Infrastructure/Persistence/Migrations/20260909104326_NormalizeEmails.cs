using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE users SET \"Email\" = LOWER(TRIM(\"Email\"));");
            migrationBuilder.Sql("UPDATE members SET \"Email\" = LOWER(TRIM(\"Email\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lowercased email values cannot be reconstructed safely.
        }
    }
}
