using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCaja.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "methodpayment",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "methodpayment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "methodpayment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "methodpayment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "attentiontype",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "attentiontype",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "attentiontype",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "attentiontype",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "attentiontype",
                keyColumn: "attention_type_id",
                keyValue: "AC",
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.UpdateData(
                table: "attentiontype",
                keyColumn: "attention_type_id",
                keyValue: "CA",
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.UpdateData(
                table: "attentiontype",
                keyColumn: "attention_type_id",
                keyValue: "CC",
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.UpdateData(
                table: "attentiontype",
                keyColumn: "attention_type_id",
                keyValue: "CS",
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.UpdateData(
                table: "attentiontype",
                keyColumn: "attention_type_id",
                keyValue: "FP",
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.UpdateData(
                table: "attentiontype",
                keyColumn: "attention_type_id",
                keyValue: "PS",
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.InsertData(
                table: "menu",
                columns: new[] { "menu_id", "created_at", "deleted_at", "display_order", "icon", "is_deleted", "name", "route", "updated_at" },
                values: new object[] { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, "router", false, "Servicios", "/services", null });

            migrationBuilder.UpdateData(
                table: "methodpayment",
                keyColumn: "method_payment_id",
                keyValue: 1,
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.UpdateData(
                table: "methodpayment",
                keyColumn: "method_payment_id",
                keyValue: 2,
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.UpdateData(
                table: "methodpayment",
                keyColumn: "method_payment_id",
                keyValue: 3,
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.UpdateData(
                table: "methodpayment",
                keyColumn: "method_payment_id",
                keyValue: 4,
                columns: new[] { "created_at", "deleted_at", "is_deleted", "updated_at" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null });

            migrationBuilder.InsertData(
                table: "rolmenu",
                columns: new[] { "menu_id", "rol_id" },
                values: new object[] { 8, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 8, 1 });

            migrationBuilder.DeleteData(
                table: "menu",
                keyColumn: "menu_id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "methodpayment");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "methodpayment");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "methodpayment");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "methodpayment");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "attentiontype");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "attentiontype");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "attentiontype");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "attentiontype");
        }
    }
}
