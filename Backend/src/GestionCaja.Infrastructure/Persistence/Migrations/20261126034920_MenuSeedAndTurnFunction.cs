using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionCaja.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MenuSeedAndTurnFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "cash",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.InsertData(
                table: "menu",
                columns: new[] { "menu_id", "created_at", "deleted_at", "display_order", "icon", "is_deleted", "name", "route", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, "home", false, "Inicio", "/home", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, "dashboard", false, "Dashboard", "/dashboard", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, "group", false, "Usuarios", "/users", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, "point_of_sale", false, "Cajas", "/cashes", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, "confirmation_number", false, "Asignación de turnos", "/turns", null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, "payments", false, "Procesos de caja", "/cash-process", null },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, "badge", false, "Clientes", "/clients", null }
                });

            migrationBuilder.InsertData(
                table: "rolmenu",
                columns: new[] { "menu_id", "rol_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 1, 2 },
                    { 3, 2 },
                    { 4, 2 },
                    { 5, 2 },
                    { 1, 3 },
                    { 6, 3 },
                    { 7, 3 }
                });

            // Función que genera la descripción del turno. El mismo script está en Backend/database.
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION fn_next_turn_description(p_attention_type_id varchar)
RETURNS varchar
LANGUAGE plpgsql
AS $$
DECLARE
    v_today date := (now() AT TIME ZONE 'America/Guayaquil')::date;
    v_next  integer;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM attentiontype WHERE attention_type_id = p_attention_type_id) THEN
        RAISE EXCEPTION 'El tipo de atención % no existe', p_attention_type_id;
    END IF;

    PERFORM pg_advisory_xact_lock(hashtext('turn_' || p_attention_type_id));

    SELECT COALESCE(MAX(RIGHT(description, 4)::integer), 0) + 1
      INTO v_next
      FROM turn
     WHERE attention_type_id = p_attention_type_id
       AND (date AT TIME ZONE 'America/Guayaquil')::date = v_today;

    IF v_next > 9999 THEN
        RAISE EXCEPTION 'Se alcanzó el máximo de turnos del día para el tipo %', p_attention_type_id;
    END IF;

    RETURN p_attention_type_id || LPAD(v_next::text, 4, '0');
END;
$$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_next_turn_description(varchar);");

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 4, 2 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 5, 2 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 6, 3 });

            migrationBuilder.DeleteData(
                table: "rolmenu",
                keyColumns: new[] { "menu_id", "rol_id" },
                keyValues: new object[] { 7, 3 });

            migrationBuilder.DeleteData(
                table: "menu",
                keyColumn: "menu_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "menu",
                keyColumn: "menu_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "menu",
                keyColumn: "menu_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "menu",
                keyColumn: "menu_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "menu",
                keyColumn: "menu_id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "menu",
                keyColumn: "menu_id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "menu",
                keyColumn: "menu_id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "cash");
        }
    }
}
