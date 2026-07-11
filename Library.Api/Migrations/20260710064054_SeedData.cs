using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Library.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "AvailableCopies", "Isbn", "PublishedYear", "Title", "TotalCopies" },
                values: new object[,]
                {
                    { 1001, "Andrew Hunt", 4, "9780201616224", 1999, "The Pragmatic Programmer", 4 },
                    { 1002, "Robert C. Martin", 2, "9780134494166", 2017, "Clean Architecture", 2 }
                });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "Email", "FullName", "IsActive", "PhoneNumber", "RegisteredDate" },
                values: new object[,]
                {
                    { 2001, "ada.seed@example.com", "Ada Lovelace", true, "555-1001", new DateTime(2026, 7, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2002, "grace.seed@example.com", "Grace Hopper", true, "555-1002", new DateTime(2026, 7, 10, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2001);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2002);
        }
    }
}
