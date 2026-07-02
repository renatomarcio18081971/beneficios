CREATE TABLE beneficios.empresas (
    id UUID PRIMARY KEY,
    razao_social VARCHAR(50) NOT NULL,
    dominio VARCHAR(20) NOT NULL,
    nome_banco VARCHAR(256),
    usuario_banco VARCHAR(256),
    senha_banco VARCHAR(256),
    data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_alteracao TIMESTAMP NULL,
    usuario_alteracao_id UUID NULL
);

CREATE INDEX idx_empresas_dominio ON beneficios.empresas(dominio);
CREATE INDEX idx_empresas_razao_social ON beneficios.empresas(razao_social);
