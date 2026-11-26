using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionCaja.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CashTurnAttention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attentionstatus",
                columns: table => new
                {
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attentionstatus", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "attentiontype",
                columns: table => new
                {
                    attention_type_id = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attentiontype", x => x.attention_type_id);
                });

            migrationBuilder.CreateTable(
                name: "cash",
                columns: table => new
                {
                    cash_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cash_description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    current_user_id = table.Column<int>(type: "integer", nullable: true),
                    opened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash", x => x.cash_id);
                    table.ForeignKey(
                        name: "fk_cash_users_current_user_id",
                        column: x => x.current_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "client",
                columns: table => new
                {
                    client_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    identification = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reference_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_client", x => x.client_id);
                });

            migrationBuilder.CreateTable(
                name: "turn",
                columns: table => new
                {
                    turn_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cash_id = table.Column<int>(type: "integer", nullable: false),
                    user_gestor_id = table.Column<int>(type: "integer", nullable: false),
                    attention_type_id = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_turn", x => x.turn_id);
                    table.CheckConstraint("ck_turn_description_format", "description ~ '^[A-Z]{2}[0-9]{4}$'");
                    table.ForeignKey(
                        name: "fk_turn_attentiontype_attention_type_id",
                        column: x => x.attention_type_id,
                        principalTable: "attentiontype",
                        principalColumn: "attention_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_turn_cash_cash_id",
                        column: x => x.cash_id,
                        principalTable: "cash",
                        principalColumn: "cash_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_turn_users_user_gestor_id",
                        column: x => x.user_gestor_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usercash",
                columns: table => new
                {
                    user_cash_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    cash_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_by = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usercash", x => x.user_cash_id);
                    table.ForeignKey(
                        name: "fk_usercash_cash_cash_id",
                        column: x => x.cash_id,
                        principalTable: "cash",
                        principalColumn: "cash_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usercash_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attention",
                columns: table => new
                {
                    attention_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    turn_id = table.Column<int>(type: "integer", nullable: false),
                    client_id = table.Column<int>(type: "integer", nullable: false),
                    attention_type_id = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    attention_status_id = table.Column<int>(type: "integer", nullable: false),
                    user_cashier_id = table.Column<int>(type: "integer", nullable: true),
                    attended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attention", x => x.attention_id);
                    table.ForeignKey(
                        name: "fk_attention_attention_types_attention_type_id",
                        column: x => x.attention_type_id,
                        principalTable: "attentiontype",
                        principalColumn: "attention_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attention_attentionstatus_attention_status_id",
                        column: x => x.attention_status_id,
                        principalTable: "attentionstatus",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attention_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "client",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attention_turns_turn_id",
                        column: x => x.turn_id,
                        principalTable: "turn",
                        principalColumn: "turn_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attention_users_user_cashier_id",
                        column: x => x.user_cashier_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "attentionstatus",
                columns: new[] { "status_id", "description" },
                values: new object[,]
                {
                    { 1, "Pendiente" },
                    { 2, "En atención" },
                    { 3, "Atendido" },
                    { 4, "Cancelado" }
                });

            migrationBuilder.InsertData(
                table: "attentiontype",
                columns: new[] { "attention_type_id", "description" },
                values: new object[,]
                {
                    { "AC", "Atención al cliente" },
                    { "CA", "Cambio de servicio" },
                    { "CC", "Cancelación de contrato" },
                    { "CS", "Contratación de servicio" },
                    { "FP", "Cambio de forma de pago" },
                    { "PS", "Pago de servicio" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_attention_attention_status_id_attended_at",
                table: "attention",
                columns: new[] { "attention_status_id", "attended_at" });

            migrationBuilder.CreateIndex(
                name: "ix_attention_attention_type_id",
                table: "attention",
                column: "attention_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_attention_client_id",
                table: "attention",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_attention_turn_id",
                table: "attention",
                column: "turn_id");

            migrationBuilder.CreateIndex(
                name: "ix_attention_user_cashier_id_attended_at",
                table: "attention",
                columns: new[] { "user_cashier_id", "attended_at" });

            migrationBuilder.CreateIndex(
                name: "ix_cash_current_user_id",
                table: "cash",
                column: "current_user_id",
                unique: true,
                filter: "current_user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_client_identification",
                table: "client",
                column: "identification",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_turn_attention_type_id_date",
                table: "turn",
                columns: new[] { "attention_type_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_turn_cash_id_date",
                table: "turn",
                columns: new[] { "cash_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_turn_user_gestor_id_date",
                table: "turn",
                columns: new[] { "user_gestor_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_usercash_cash_id",
                table: "usercash",
                column: "cash_id");

            migrationBuilder.CreateIndex(
                name: "ix_usercash_user_id_cash_id",
                table: "usercash",
                columns: new[] { "user_id", "cash_id" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attention");

            migrationBuilder.DropTable(
                name: "usercash");

            migrationBuilder.DropTable(
                name: "attentionstatus");

            migrationBuilder.DropTable(
                name: "client");

            migrationBuilder.DropTable(
                name: "turn");

            migrationBuilder.DropTable(
                name: "attentiontype");

            migrationBuilder.DropTable(
                name: "cash");
        }
    }
}
