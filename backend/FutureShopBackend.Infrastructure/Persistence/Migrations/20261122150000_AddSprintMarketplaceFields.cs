using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutureShopBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSprintMarketplaceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_reviews_single_target",
                table: "reviews");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "wishlists",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<decimal>(
                name: "price_at_addition",
                table: "wishlists",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "cons",
                table: "reviews",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pros",
                table: "reviews",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "seller_replied_at",
                table: "reviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seller_reply",
                table: "reviews",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "condition",
                table: "products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "new");

            migrationBuilder.AddColumn<decimal>(
                name: "rating",
                table: "products",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "reviews_count",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "AspNetUsers",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_iban",
                table: "AspNetUsers",
                type: "character varying(34)",
                maxLength: 34,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_name",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "facebook_url",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "instagram_url",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_language",
                table: "AspNetUsers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "uk");

            migrationBuilder.AddColumn<string>(
                name: "preferred_theme",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<string>(
                name: "tax_identification_code",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telegram_url",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verification_status",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "notsubmitted");

            migrationBuilder.AddColumn<DateTime>(
                name: "verification_submitted_at",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "website_url",
                table: "AspNetUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_reviews_single_target",
                table: "reviews",
                sql: "num_nonnulls(product_id, seller_id) >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_reviews_single_target",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "wishlists");

            migrationBuilder.DropColumn(
                name: "price_at_addition",
                table: "wishlists");

            migrationBuilder.DropColumn(
                name: "cons",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "pros",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "seller_replied_at",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "seller_reply",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "condition",
                table: "products");

            migrationBuilder.DropColumn(
                name: "rating",
                table: "products");

            migrationBuilder.DropColumn(
                name: "reviews_count",
                table: "products");

            migrationBuilder.DropColumn(
                name: "address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "bank_iban",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "company_name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "facebook_url",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "instagram_url",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "preferred_language",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "preferred_theme",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "tax_identification_code",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "telegram_url",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "verification_status",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "verification_submitted_at",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "website_url",
                table: "AspNetUsers");

            migrationBuilder.AddCheckConstraint(
                name: "ck_reviews_single_target",
                table: "reviews",
                sql: "num_nonnulls(product_id, seller_id) = 1");
        }
    }
}
