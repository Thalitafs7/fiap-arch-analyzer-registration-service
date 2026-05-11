# Configuração de Envio de Email - ms-ordens

## Visão Geral

O microserviço ms-ordens possui funcionalidade de envio de emails para notificações de clientes, seguindo o mesmo padrão implementado no projeto original MecanicaOS.

## Dependências

- **SendGrid** (v9.29.3) - Provedor de envio de emails

## Configuração

### 1. API Key do SendGrid

Adicione a chave de API do SendGrid no arquivo `appsettings.json` ou como variável de ambiente:

```json
{
  "SENDGRID_APIKEY": "SG.your-sendgrid-api-key-here"
}
```

**Variável de Ambiente:**
```bash
export SENDGRID_APIKEY="SG.your-sendgrid-api-key-here"
```

## Funcionalidades

### 1. Email de Orçamento Gerado

**Quando é enviado:**
- Após geração de orçamento via comando `GerarOrcamentoCommand`
- Evento de domínio: `OrcamentoGeradoEvent`
- Handler: `OrcamentoGeradoHandler`

**Template:**
- Localização: `src/API/Templates/EmailOrcamentoOS.html`
- Variáveis:
  - `{{NOME_CLIENTE}}`
  - `{{NOME_SERVICO}}`
  - `{{VALOR_SERVICO}}`
  - `{{INSUMOS}}` (lista dinâmica)
  - `{{VALOR_TOTAL}}`

**Destinatário:**
- Email do cliente obtido via ms-cadastros

### 2. Email de Ordem de Serviço Finalizada

**Quando é enviado:**
- Após confirmação de pagamento aprovado via comando `ConfirmarPagamentoCommand`
- Evento de domínio: `OrdemServicoFinalizadaEvent`
- Handler: `OrdemServicoFinalizadaHandler`

**Template:**
- Localização: `src/API/Templates/EmailOrdemServicoFinalizada.html`
- Variáveis:
  - `{{NOME_CLIENTE}}`
  - `{{NOME_SERVICO}}`
  - `{{MODELO_VEICULO}}`
  - `{{PLACA_VEICULO}}`

**Destinatário:**
- Email do cliente obtido via ms-cadastros

## Estrutura de Arquivos

```
ms-ordens/
├── src/
│   ├── Application/
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   └── IEmailService.cs
│   │   │   └── DTOs/
│   │   │       ├── ClienteDto.cs
│   │   │       └── VeiculoDto.cs
│   │   ├── DomainEvents/
│   │   │   ├── OrcamentoGeradoEvent.cs
│   │   │   ├── OrdemServicoFinalizadaEvent.cs
│   │   │   └── Handlers/
│   │   │       ├── OrcamentoGeradoHandler.cs
│   │   │       └── OrdemServicoFinalizadaHandler.cs
│   │   └── Commands/
│   │       ├── GerarOrcamento/
│   │       │   └── GerarOrcamentoHandler.cs (publica evento)
│   │       └── ConfirmarPagamento/
│   │           └── ConfirmarPagamentoHandler.cs (publica evento)
│   ├── Infrastructure/
│   │   └── Services/
│   │       └── EmailService.cs
│   └── API/
│       └── Templates/
│           ├── EmailOrcamentoOS.html
│           └── EmailOrdemServicoFinalizada.html
```

## Integração com ms-cadastros

O ms-ordens busca dados do cliente e veículo via HTTP do ms-cadastros:

- `ICadastrosService.ObterClienteAsync(clienteId)` - Retorna nome e email
- `ICadastrosService.ObterVeiculoAsync(veiculoId)` - Retorna modelo e placa
- `ICadastrosService.ObterServicoAsync(servicoId)` - Retorna nome do serviço

**Cache:**
- Implementado via `CachedCadastrosService`
- TTL: 10 minutos
- Fallback para cache stale em caso de indisponibilidade

## Fluxo de Eventos

### Orçamento Gerado
```
1. Controller recebe requisição POST /api/ordens/{id}/orcamento
2. GerarOrcamentoHandler processa comando
3. Commit no banco de dados
4. Publica OrcamentoGeradoEvent (MediatR)
5. OrcamentoGeradoHandler recebe evento
6. Busca dados do cliente/serviço no ms-cadastros
7. Gera HTML do email a partir do template
8. Envia email via SendGrid
```

### Ordem Finalizada
```
1. Webhook do Mercado Pago chama POST /api/ordens/{id}/pagamento/confirmar
2. ConfirmarPagamentoHandler processa comando
3. Aprova pagamento e finaliza ordem
4. Commit no banco de dados
5. Publica OrdemServicoFinalizadaEvent (MediatR)
6. OrdemServicoFinalizadaHandler recebe evento
7. Busca dados do cliente/veículo/serviço no ms-cadastros
8. Gera HTML do email a partir do template
9. Envia email via SendGrid
```

## Logs

O serviço registra logs estruturados para todas as operações:

- `LogInformation`: Início e sucesso do envio
- `LogWarning`: Dados incompletos (cliente/veículo não encontrado)
- `LogError`: Falhas no envio ou na busca de dados

## Resiliência

**Tratamento de Erros:**
- Handlers de eventos capturam exceções e logam
- Falha no envio de email **não impede** a conclusão da ordem
- Circuit breaker e retry aplicados nas chamadas ao ms-cadastros

**Cache:**
- Reduz latência e melhora resiliência
- Fallback para dados stale em caso de indisponibilidade do ms-cadastros

## Testes

Para testar o envio de emails em desenvolvimento:

### Orçamento
```bash
POST /api/ordens/{id}/orcamento
{
  "valorServico": 150.00,
  "valorInsumos": 80.00,
  "diasValidade": 7
}
```

### Ordem Finalizada
```bash
POST /api/ordens/{id}/pagamento/confirmar
{
  "status": "approved",
  "paymentId": "123456789"
}
```

## Troubleshooting

**Email não está sendo enviado:**
- Verifique se `SENDGRID_APIKEY` está configurada
- Verifique logs dos handlers de eventos
- Verifique se ms-cadastros está respondendo corretamente
- Verifique se os templates HTML existem no diretório correto

**Cliente não recebe email:**
- Verifique se o cliente possui email cadastrado no ms-cadastros
- Verifique logs para erros de autenticação SendGrid
- Verifique spam/lixeira do cliente

**Dados incompletos no email:**
- Verifique se ms-cadastros retorna todos os dados necessários
- Verifique cache do CachedCadastrosService
- Verifique logs de warning sobre dados incompletos
