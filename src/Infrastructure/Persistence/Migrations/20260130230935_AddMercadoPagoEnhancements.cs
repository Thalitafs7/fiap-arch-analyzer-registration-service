using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{

    public partial class AddMercadoPagoEnhancements : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "pagamentos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pagador_sobrenome",
                table: "pagamentos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pagador_tipo_documento",
                table: "pagamentos",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pix_ticket_url",
                table: "pagamentos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "pagamentos");

            migrationBuilder.DropColumn(
                name: "pagador_sobrenome",
                table: "pagamentos");

            migrationBuilder.DropColumn(
                name: "pagador_tipo_documento",
                table: "pagamentos");

            migrationBuilder.DropColumn(
                name: "pix_ticket_url",
                table: "pagamentos");
        }
    }
}
