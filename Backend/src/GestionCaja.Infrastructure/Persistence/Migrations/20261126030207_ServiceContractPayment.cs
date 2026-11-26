using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionCaja.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ServiceContractPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "methodpayment",
                columns: table => new
                {
                    method_payment_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_methodpayment", x => x.method_payment_id);
                });

            migrationBuilder.CreateTable(
                name: "service",
                columns: table => new
                {
                    service_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    service_description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    speed_mbps = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service", x => x.service_id);
                    table.CheckConstraint("ck_service_price", "price >= 0");
                    table.CheckConstraint("ck_service_speed", "speed_mbps > 0");
                });

            migrationBuilder.CreateTable(
                name: "statuscontract",
                columns: table => new
                {
                    status_id = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_statuscontract", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "device",
                columns: table => new
                {
                    device_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    device_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device", x => x.device_id);
                    table.ForeignKey(
                        name: "fk_device_services_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contract",
                columns: table => new
                {
                    contract_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    client_id = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    status_contract_id = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    method_payment_id = table.Column<int>(type: "integer", nullable: false),
                    previous_contract_id = table.Column<int>(type: "integer", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contract", x => x.contract_id);
                    table.CheckConstraint("ck_contract_dates", "end_date >= start_date");
                    table.ForeignKey(
                        name: "fk_contract_client_client_id",
                        column: x => x.client_id,
                        principalTable: "client",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contract_contract_previous_contract_id",
                        column: x => x.previous_contract_id,
                        principalTable: "contract",
                        principalColumn: "contract_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contract_method_payments_method_payment_id",
                        column: x => x.method_payment_id,
                        principalTable: "methodpayment",
                        principalColumn: "method_payment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contract_services_service_id",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contract_statuscontract_status_contract_id",
                        column: x => x.status_contract_id,
                        principalTable: "statuscontract",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    client_id = table.Column<int>(type: "integer", nullable: false),
                    contract_id = table.Column<int>(type: "integer", nullable: false),
                    method_payment_id = table.Column<int>(type: "integer", nullable: false),
                    user_cashier_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.payment_id);
                    table.CheckConstraint("ck_payments_amount", "amount > 0");
                    table.ForeignKey(
                        name: "fk_payments_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "client",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payments_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contract",
                        principalColumn: "contract_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payments_method_payments_method_payment_id",
                        column: x => x.method_payment_id,
                        principalTable: "methodpayment",
                        principalColumn: "method_payment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payments_users_user_cashier_id",
                        column: x => x.user_cashier_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "methodpayment",
                columns: new[] { "method_payment_id", "description" },
                values: new object[,]
                {
                    { 1, "Efectivo" },
                    { 2, "Tarjeta de crédito" },
                    { 3, "Tarjeta de débito" },
                    { 4, "Transferencia bancaria" }
                });

            migrationBuilder.InsertData(
                table: "service",
                columns: new[] { "service_id", "active", "created_at", "deleted_at", "is_deleted", "price", "service_description", "service_name", "speed_mbps", "updated_at" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 25.00m, "Internet fibra óptica para el hogar", "Plan Hogar 100", 100, null },
                    { 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 35.00m, "Internet fibra óptica para streaming y teletrabajo", "Plan Hogar 300", 300, null },
                    { 3, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 60.00m, "Internet fibra óptica para pequeños negocios", "Plan Pyme 600", 600, null }
                });

            migrationBuilder.InsertData(
                table: "statuscontract",
                columns: new[] { "status_id", "description" },
                values: new object[,]
                {
                    { "CAN", "Contrato cancelado" },
                    { "REN", "Renovación de servicio" },
                    { "SUS", "Contrato sustituido" },
                    { "VIG", "Contrato vigente" }
                });

            migrationBuilder.InsertData(
                table: "device",
                columns: new[] { "device_id", "created_at", "deleted_at", "device_name", "is_deleted", "service_id", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Router WiFi 5", false, 1, null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ONT fibra óptica", false, 1, null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Router WiFi 6", false, 2, null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ONT fibra óptica", false, 2, null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Router WiFi 6 empresarial", false, 3, null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ONT fibra óptica", false, 3, null },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Repetidor mesh", false, 3, null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_contract_client_id_status_contract_id",
                table: "contract",
                columns: new[] { "client_id", "status_contract_id" });

            migrationBuilder.CreateIndex(
                name: "ix_contract_method_payment_id",
                table: "contract",
                column: "method_payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_contract_previous_contract_id",
                table: "contract",
                column: "previous_contract_id");

            migrationBuilder.CreateIndex(
                name: "ix_contract_service_id",
                table: "contract",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_contract_status_contract_id",
                table: "contract",
                column: "status_contract_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_service_id",
                table: "device",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_client_id_payment_date",
                table: "payments",
                columns: new[] { "client_id", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_contract_id",
                table: "payments",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_method_payment_id",
                table: "payments",
                column: "method_payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_user_cashier_id_payment_date",
                table: "payments",
                columns: new[] { "user_cashier_id", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "ix_service_service_name",
                table: "service",
                column: "service_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "contract");

            migrationBuilder.DropTable(
                name: "methodpayment");

            migrationBuilder.DropTable(
                name: "service");

            migrationBuilder.DropTable(
                name: "statuscontract");
        }
    }
}
