using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutureShopBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "id", "concurrency_stamp", "name", "normalized_name" },
                values: new object[] { 4, "role-guest-v1", "guest", "GUEST" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: 4);
        }
    }
}
