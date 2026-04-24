-- Migration V1: Initial Schema
-- Criado em: 2023-10-27

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS ordens_servico (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cliente_id UUID NOT NULL,
    veiculo_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    valor_total DECIMAL(18, 2) DEFAULT 0.00,
    criado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    atualizado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS orcamentos (
    id SERIAL PRIMARY KEY,
    ordem_id UUID NOT NULL REFERENCES ordens_servico(id) ON DELETE CASCADE,
    valor_total DECIMAL(18, 2) DEFAULT 0.00,
    expira_em TIMESTAMP WITH TIME ZONE,
    status VARCHAR(50) NOT NULL,
    criado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS itens_orcamento (
    id SERIAL PRIMARY KEY,
    orcamento_id INTEGER NOT NULL REFERENCES orcamentos(id) ON DELETE CASCADE,
    servico_id UUID NOT NULL,
    descricao TEXT,
    quantidade DECIMAL(18, 2) NOT NULL,
    preco_unitario DECIMAL(18, 2) NOT NULL,
    subtotal DECIMAL(18, 2) GENERATED ALWAYS AS (quantidade * preco_unitario) STORED
);

CREATE TABLE IF NOT EXISTS insumos_ordem (
    id SERIAL PRIMARY KEY,
    ordem_id UUID NOT NULL REFERENCES ordens_servico(id) ON DELETE CASCADE,
    produto_id UUID NOT NULL,
    quantidade DECIMAL(18, 2) NOT NULL,
    status VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS pagamentos (
    id SERIAL PRIMARY KEY,
    ordem_id UUID NOT NULL REFERENCES ordens_servico(id) ON DELETE CASCADE,
    gateway VARCHAR(50),
    external_id VARCHAR(100),
    valor DECIMAL(18, 2) NOT NULL,
    status VARCHAR(50) NOT NULL,
    criado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    confirmado_em TIMESTAMP WITH TIME ZONE
);

CREATE TABLE IF NOT EXISTS historico_ordem (
    id SERIAL PRIMARY KEY,
    ordem_id UUID NOT NULL REFERENCES ordens_servico(id) ON DELETE CASCADE,
    evento VARCHAR(100) NOT NULL,
    payload_json JSONB,
    criado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
