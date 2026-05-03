using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class update_analise_error : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "error",
                newName: "tipo");

            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "error",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "error",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "ClienteId",
                table: "analise",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "analise",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "analise");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "analise");

            migrationBuilder.RenameColumn(
                name: "tipo",
                table: "error",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "error",
                newName: "Descricao");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "error",
                newName: "id");
        }
    }
}
