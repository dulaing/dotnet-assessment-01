using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleUserPerMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_MemberId",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "IX_users_MemberId",
                table: "users",
                column: "MemberId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_MemberId",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "IX_users_MemberId",
                table: "users",
                column: "MemberId");
        }
    }
}
