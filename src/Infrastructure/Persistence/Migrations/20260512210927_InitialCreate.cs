using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analise",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analise", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "error",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "diagrama",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    TipoDiagrama = table.Column<string>(type: "text", nullable: true),
                    URLS3Diagrama = table.Column<string>(type: "text", nullable: true),
                    IdRelatorio = table.Column<Guid>(type: "uuid", nullable: false),
                    analise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagrama", x => x.id);
                    table.ForeignKey(
                        name: "FK_diagrama_analise_analise_id",
                        column: x => x.analise_id,
                        principalTable: "analise",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "relatorio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    soat_analysis_id = table.Column<Guid>(type: "uuid", nullable: true),
                    id_diagrama = table.Column<Guid>(type: "uuid", nullable: true),
                    componentes_identificado = table.Column<string>(type: "jsonb", nullable: true),
                    risco_arquitetura = table.Column<string>(type: "jsonb", nullable: true),
                    recomendacao = table.Column<string>(type: "jsonb", nullable: true),
                    message_error = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relatorio", x => x.id);
                    table.ForeignKey(
                        name: "FK_relatorio_diagrama_id_diagrama",
                        column: x => x.id_diagrama,
                        principalTable: "diagrama",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_diagrama_analise_id",
                table: "diagrama",
                column: "analise_id");

            migrationBuilder.CreateIndex(
                name: "IX_relatorio_id_diagrama",
                table: "relatorio",
                column: "id_diagrama",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "error");

            migrationBuilder.DropTable(
                name: "relatorio");

            migrationBuilder.DropTable(
                name: "diagrama");

            migrationBuilder.DropTable(
                name: "analise");
        }
    }
}
