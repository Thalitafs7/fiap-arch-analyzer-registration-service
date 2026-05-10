using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class atualizarelatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_diagrama_analise_AnaliseId",
                table: "diagrama");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "relatorio",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "urls3_relatorio",
                table: "relatorio",
                newName: "message_error");

            migrationBuilder.RenameColumn(
                name: "AnaliseId",
                table: "diagrama",
                newName: "analise_id");

            migrationBuilder.RenameIndex(
                name: "IX_diagrama_AnaliseId",
                table: "diagrama",
                newName: "IX_diagrama_analise_id");

            migrationBuilder.AddColumn<string>(
                name: "componentes_identificado",
                table: "relatorio",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "recomendacao",
                table: "relatorio",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "risco_arquitetura",
                table: "relatorio",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "soat_analysis_id",
                table: "relatorio",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "analise_id",
                table: "diagrama",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_diagrama_analise_analise_id",
                table: "diagrama",
                column: "analise_id",
                principalTable: "analise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_diagrama_analise_analise_id",
                table: "diagrama");

            migrationBuilder.DropColumn(
                name: "componentes_identificado",
                table: "relatorio");

            migrationBuilder.DropColumn(
                name: "recomendacao",
                table: "relatorio");

            migrationBuilder.DropColumn(
                name: "risco_arquitetura",
                table: "relatorio");

            migrationBuilder.DropColumn(
                name: "soat_analysis_id",
                table: "relatorio");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "relatorio",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "message_error",
                table: "relatorio",
                newName: "urls3_relatorio");

            migrationBuilder.RenameColumn(
                name: "analise_id",
                table: "diagrama",
                newName: "AnaliseId");

            migrationBuilder.RenameIndex(
                name: "IX_diagrama_analise_id",
                table: "diagrama",
                newName: "IX_diagrama_AnaliseId");

            migrationBuilder.AlterColumn<Guid>(
                name: "AnaliseId",
                table: "diagrama",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_diagrama_analise_AnaliseId",
                table: "diagrama",
                column: "AnaliseId",
                principalTable: "analise",
                principalColumn: "id");
        }
    }
}
