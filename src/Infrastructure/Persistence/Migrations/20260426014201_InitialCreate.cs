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
                    nome = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: true),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error", x => x.id);
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
                    AnaliseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagrama", x => x.id);
                    table.ForeignKey(
                        name: "FK_diagrama_analise_AnaliseId",
                        column: x => x.AnaliseId,
                        principalTable: "analise",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "relatorio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    URLS3Relatorio = table.Column<string>(type: "text", nullable: true),
                    DiagramaId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdDiagrama = table.Column<Guid>(type: "uuid", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relatorio", x => x.id);
                    table.ForeignKey(
                        name: "FK_relatorio_diagrama_DiagramaId",
                        column: x => x.DiagramaId,
                        principalTable: "diagrama",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_relatorio_diagrama_IdDiagrama",
                        column: x => x.IdDiagrama,
                        principalTable: "diagrama",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_diagrama_AnaliseId",
                table: "diagrama",
                column: "AnaliseId");

            migrationBuilder.CreateIndex(
                name: "IX_relatorio_DiagramaId",
                table: "relatorio",
                column: "DiagramaId");

            migrationBuilder.CreateIndex(
                name: "IX_relatorio_IdDiagrama",
                table: "relatorio",
                column: "IdDiagrama",
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
