using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updatemap_relatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_relatorio_diagrama_DiagramaId",
                table: "relatorio");

            migrationBuilder.DropForeignKey(
                name: "FK_relatorio_diagrama_IdDiagrama",
                table: "relatorio");

            migrationBuilder.DropIndex(
                name: "IX_relatorio_DiagramaId",
                table: "relatorio");

            migrationBuilder.DropColumn(
                name: "DiagramaId",
                table: "relatorio");

            migrationBuilder.RenameColumn(
                name: "URLS3Relatorio",
                table: "relatorio",
                newName: "urls3_relatorio");

            migrationBuilder.RenameColumn(
                name: "IdDiagrama",
                table: "relatorio",
                newName: "id_diagrama");

            migrationBuilder.RenameIndex(
                name: "IX_relatorio_IdDiagrama",
                table: "relatorio",
                newName: "IX_relatorio_id_diagrama");

            migrationBuilder.AddForeignKey(
                name: "FK_relatorio_diagrama_id_diagrama",
                table: "relatorio",
                column: "id_diagrama",
                principalTable: "diagrama",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_relatorio_diagrama_id_diagrama",
                table: "relatorio");

            migrationBuilder.RenameColumn(
                name: "urls3_relatorio",
                table: "relatorio",
                newName: "URLS3Relatorio");

            migrationBuilder.RenameColumn(
                name: "id_diagrama",
                table: "relatorio",
                newName: "IdDiagrama");

            migrationBuilder.RenameIndex(
                name: "IX_relatorio_id_diagrama",
                table: "relatorio",
                newName: "IX_relatorio_IdDiagrama");

            migrationBuilder.AddColumn<Guid>(
                name: "DiagramaId",
                table: "relatorio",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_relatorio_DiagramaId",
                table: "relatorio",
                column: "DiagramaId");

            migrationBuilder.AddForeignKey(
                name: "FK_relatorio_diagrama_DiagramaId",
                table: "relatorio",
                column: "DiagramaId",
                principalTable: "diagrama",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_relatorio_diagrama_IdDiagrama",
                table: "relatorio",
                column: "IdDiagrama",
                principalTable: "diagrama",
                principalColumn: "id");
        }
    }
}
