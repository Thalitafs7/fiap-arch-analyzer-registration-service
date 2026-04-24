using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OrdensDbContext))]
    partial class OrdensDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.EventoProcessado", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("Ativo")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("DataAtualizacao")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("DataCadastro")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EventId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("event_id");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("event_type");

                    b.Property<Guid?>("OrdemServicoId")
                        .HasColumnType("uuid")
                        .HasColumnName("ordem_servico_id");

                    b.Property<DateTime>("ProcessedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_at");

                    b.HasKey("Id");

                    b.HasIndex("EventId")
                        .IsUnique();

                    b.ToTable("eventos_processados", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.InsumoOS", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("Ativo")
                        .HasColumnType("boolean")
                        .HasColumnName("ativo");

                    b.Property<DateTime?>("DataAtualizacao")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_atualizacao");

                    b.Property<DateTime>("DataCadastro")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_cadastro");

                    b.Property<Guid>("EstoqueId")
                        .HasColumnType("uuid")
                        .HasColumnName("estoque_id");

                    b.Property<string>("Insumo")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("insumo");

                    b.Property<Guid>("OrdemServicoId")
                        .HasColumnType("uuid")
                        .HasColumnName("ordem_servico_id");

                    b.Property<decimal>("PrecoUnitario")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("preco_unitario");

                    b.Property<int>("Quantidade")
                        .HasColumnType("integer")
                        .HasColumnName("quantidade");

                    b.HasKey("Id");

                    b.HasIndex("OrdemServicoId");

                    b.ToTable("insumos_os", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Orcamento", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool?>("Aceito")
                        .HasColumnType("boolean")
                        .HasColumnName("aceito");

                    b.Property<bool>("Ativo")
                        .HasColumnType("boolean")
                        .HasColumnName("ativo");

                    b.Property<DateTime?>("DataAtualizacao")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_atualizacao");

                    b.Property<DateTime>("DataCadastro")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_cadastro");

                    b.Property<DateTime?>("DataResposta")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_resposta");

                    b.Property<DateTime>("DataValidade")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_validade");

                    b.Property<Guid>("OrdemServicoId")
                        .HasColumnType("uuid")
                        .HasColumnName("ordem_servico_id");

                    b.Property<decimal>("ValorInsumos")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("valor_insumos");

                    b.Property<decimal>("ValorServico")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("valor_servico");

                    b.HasKey("Id");

                    b.HasIndex("OrdemServicoId")
                        .IsUnique();

                    b.ToTable("orcamentos", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.OrdemServico", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("Ativo")
                        .HasColumnType("boolean")
                        .HasColumnName("ativo");

                    b.Property<Guid>("ClienteId")
                        .HasColumnType("uuid")
                        .HasColumnName("cliente_id");

                    b.Property<DateTime>("DataAbertura")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_abertura");

                    b.Property<DateTime?>("DataAtualizacao")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_atualizacao");

                    b.Property<DateTime>("DataCadastro")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_cadastro");

                    b.Property<DateTime?>("DataFinalizacao")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_finalizacao");

                    b.Property<string>("Descricao")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)")
                        .HasColumnName("descricao");

                    b.Property<string>("Observacoes")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("observacoes");

                    b.Property<string>("SagaError")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("saga_error");

                    b.Property<int>("SagaStatus")
                        .HasColumnType("integer")
                        .HasColumnName("saga_status");

                    b.Property<Guid>("ServicoId")
                        .HasColumnType("uuid")
                        .HasColumnName("servico_id");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<Guid>("VeiculoId")
                        .HasColumnType("uuid")
                        .HasColumnName("veiculo_id");

                    b.HasKey("Id");

                    b.HasIndex("ClienteId");

                    b.HasIndex("Status");

                    b.ToTable("ordens_servico", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("Ativo")
                        .HasColumnType("boolean");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime?>("DataAtualizacao")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("DataCadastro")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Error")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("ProcessedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("RetryCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("DataCadastro")
                        .HasDatabaseName("IX_OutboxMessages_DataCadastro");

                    b.HasIndex("ProcessedAt", "RetryCount")
                        .HasDatabaseName("IX_OutboxMessages_Pending")
                        .HasFilter("\"ProcessedAt\" IS NULL AND \"RetryCount\" < 5");

                    b.ToTable("OutboxMessages", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.Pagamento", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("Ativo")
                        .HasColumnType("boolean")
                        .HasColumnName("ativo");

                    b.Property<DateTime?>("DataAprovacao")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_aprovacao");

                    b.Property<DateTime?>("DataAtualizacao")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_atualizacao");

                    b.Property<DateTime>("DataCadastro")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_cadastro");

                    b.Property<DateTime?>("DataCancelamento")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_cancelamento");

                    b.Property<DateTime>("DataCriacao")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("data_criacao");

                    b.Property<string>("IdempotencyKey")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("idempotency_key");

                    b.Property<string>("JsonResponse")
                        .HasColumnType("text")
                        .HasColumnName("json_response");

                    b.Property<string>("MercadoPagoPaymentId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("mp_payment_id");

                    b.Property<string>("MercadoPagoPreferenceId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("mp_preference_id");

                    b.Property<int>("MetodoPagamento")
                        .HasColumnType("integer")
                        .HasColumnName("metodo_pagamento");

                    b.Property<Guid>("OrdemServicoId")
                        .HasColumnType("uuid")
                        .HasColumnName("ordem_servico_id");

                    b.Property<string>("PagadorDocumento")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("pagador_documento");

                    b.Property<string>("PagadorEmail")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("pagador_email");

                    b.Property<string>("PagadorNome")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("pagador_nome");

                    b.Property<string>("PagadorSobrenome")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("pagador_sobrenome");

                    b.Property<string>("PagadorTipoDocumento")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("pagador_tipo_documento");

                    b.Property<DateTime?>("PixExpiracao")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("pix_expiracao");

                    b.Property<string>("PixQrCode")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("pix_qr_code");

                    b.Property<string>("PixQrCodeBase64")
                        .HasColumnType("text")
                        .HasColumnName("pix_qr_code_base64");

                    b.Property<string>("PixTicketUrl")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("pix_ticket_url");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<decimal>("Valor")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("valor");

                    b.HasKey("Id");

                    b.HasIndex("MercadoPagoPaymentId");

                    b.HasIndex("OrdemServicoId")
                        .IsUnique();

                    b.ToTable("pagamentos", (string)null);
                });

            modelBuilder.Entity("Domain.Entities.InsumoOS", b =>
                {
                    b.HasOne("Domain.Entities.OrdemServico", null)
                        .WithMany("Insumos")
                        .HasForeignKey("OrdemServicoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.Orcamento", b =>
                {
                    b.HasOne("Domain.Entities.OrdemServico", null)
                        .WithOne("Orcamento")
                        .HasForeignKey("Domain.Entities.Orcamento", "OrdemServicoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.Pagamento", b =>
                {
                    b.HasOne("Domain.Entities.OrdemServico", null)
                        .WithOne("Pagamento")
                        .HasForeignKey("Domain.Entities.Pagamento", "OrdemServicoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.OrdemServico", b =>
                {
                    b.Navigation("Insumos");

                    b.Navigation("Orcamento");

                    b.Navigation("Pagamento");
                });
#pragma warning restore 612, 618
        }
    }
}
