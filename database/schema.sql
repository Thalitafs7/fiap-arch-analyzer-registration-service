-- Habilitar extensão UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tabela: ordens_servico
CREATE TABLE ordens_servico (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cliente_id UUID NOT NULL,
    veiculo_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    valor_total DECIMAL(18, 2) DEFAULT 0.00,
    criado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    atualizado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: orcamentos
CREATE TABLE orcamentos (
    id SERIAL PRIMARY KEY,
    ordem_id UUID NOT NULL REFERENCES ordens_servico(id) ON DELETE CASCADE,
    valor_total DECIMAL(18, 2) DEFAULT 0.00,
    expira_em TIMESTAMP WITH TIME ZONE,
    status VARCHAR(50) NOT NULL,
    criado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: itens_orcamento
CREATE TABLE itens_orcamento (
    id SERIAL PRIMARY KEY,
    orcamento_id INTEGER NOT NULL REFERENCES orcamentos(id) ON DELETE CASCADE,
    servico_id UUID NOT NULL,
    descricao TEXT,
    quantidade DECIMAL(18, 2) NOT NULL,
    preco_unitario DECIMAL(18, 2) NOT NULL,
    subtotal DECIMAL(18, 2) GENERATED ALWAYS AS (quantidade * preco_unitario) STORED
);

-- Tabela: insumos_ordem
CREATE TABLE insumos_ordem (
    id SERIAL PRIMARY KEY,
    ordem_id UUID NOT NULL REFERENCES ordens_servico(id) ON DELETE CASCADE,
    produto_id UUID NOT NULL,
    quantidade DECIMAL(18, 2) NOT NULL,
    status VARCHAR(50) NOT NULL
);

-- Tabela: pagamentos
CREATE TABLE pagamentos (
    id SERIAL PRIMARY KEY,
    ordem_id UUID NOT NULL REFERENCES ordens_servico(id) ON DELETE CASCADE,
    gateway VARCHAR(50),
    external_id VARCHAR(100),
    valor DECIMAL(18, 2) NOT NULL,
    status VARCHAR(50) NOT NULL,
    criado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    confirmado_em TIMESTAMP WITH TIME ZONE
);

-- Tabela: historico_ordem
CREATE TABLE historico_ordem (
    id SERIAL PRIMARY KEY,
    ordem_id UUID NOT NULL REFERENCES ordens_servico(id) ON DELETE CASCADE,
    evento VARCHAR(100) NOT NULL,
    payload_json JSONB,
    criado_em TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para performance
CREATE INDEX idx_ordens_cliente ON ordens_servico(cliente_id);
CREATE INDEX idx_orcamentos_ordem ON orcamentos(ordem_id);
CREATE INDEX idx_pagamentos_ordem ON pagamentos(ordem_id);
CREATE INDEX idx_historico_ordem ON historico_ordem(ordem_id);
