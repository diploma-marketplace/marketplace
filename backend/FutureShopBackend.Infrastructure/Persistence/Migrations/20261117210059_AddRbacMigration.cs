using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutureShopBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRbacMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "item_status",
                table: "order_items");

            migrationBuilder.RenameColumn(
                name: "status_type",
                table: "AspNetUsers",
                newName: "seller_type");

            migrationBuilder.AlterColumn<string>(
                name: "payment_status",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "unpaid");

            migrationBuilder.AlterColumn<string>(
                name: "order_status",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "created",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "new");

            migrationBuilder.AddColumn<string>(
                name: "order_status",
                table: "order_items",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "order_status",
                table: "order_items");

            migrationBuilder.RenameColumn(
                name: "seller_type",
                table: "AspNetUsers",
                newName: "status_type");

            migrationBuilder.AlterColumn<string>(
                name: "payment_status",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "unpaid",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "pending");

            migrationBuilder.AlterColumn<string>(
                name: "order_status",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "new",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "created");

            migrationBuilder.AddColumn<string>(
                name: "item_status",
                table: "order_items",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "new");
        }
    }
}
