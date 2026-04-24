using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{

    public partial class InitialCreate : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eventos_processados",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_processados", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordens_servico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    saga_status = table.Column<int>(type: "integer", nullable: false),
                    saga_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    data_abertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_finalizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordens_servico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "insumos_os",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estoque_id = table.Column<Guid>(type: "uuid", nullable: false),
                    insumo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insumos_os", x => x.id);
                    table.ForeignKey(
                        name: "FK_insumos_os_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orcamentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_servico = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_insumos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    data_validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    aceito = table.Column<bool>(type: "boolean", nullable: true),
                    data_resposta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orcamentos", x => x.id);
                    table.ForeignKey(
                        name: "FK_orcamentos_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    metodo_pagamento = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    mp_preference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mp_payment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pix_qr_code = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    pix_qr_code_base64 = table.Column<string>(type: "text", nullable: true),
                    pix_expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pagador_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pagador_nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pagador_documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    json_response = table.Column<string>(type: "text", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_aprovacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_cancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagamentos", x => x.id);
                    table.ForeignKey(
                        name: "FK_pagamentos_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_eventos_processados_event_id",
                table: "eventos_processados",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_insumos_os_ordem_servico_id",
                table: "insumos_os",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "IX_orcamentos_ordem_servico_id",
                table: "orcamentos",
                column: "ordem_servico_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_cliente_id",
                table: "ordens_servico",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_status",
                table: "ordens_servico",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_mp_payment_id",
                table: "pagamentos",
                column: "mp_payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_ordem_servico_id",
                table: "pagamentos",
                column: "ordem_servico_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eventos_processados");

            migrationBuilder.DropTable(
                name: "insumos_os");

            migrationBuilder.DropTable(
                name: "orcamentos");

            migrationBuilder.DropTable(
                name: "pagamentos");

            migrationBuilder.DropTable(
                name: "ordens_servico");
        }
    }
}
