CREATE TABLE beneficios.usuarios (
    id UUID PRIMARY KEY,
    nome VARCHAR(50) NOT NULL,
    senha VARCHAR(50) NOT NULL,
    email VARCHAR(256) NOT NULL,
    perfil VARCHAR(20) NOT NULL DEFAULT 'Empresa',
    empresa_id UUID NOT NULL,
    token TEXT NULL,
    data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_alteracao TIMESTAMP NULL,
    usuario_alteracao_id UUID NULL,
    CONSTRAINT fk_usuarios_empresa FOREIGN KEY (empresa_id) REFERENCES beneficios.empresas(id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX idx_usuarios_email ON beneficios.usuarios(email);
CREATE INDEX idx_usuarios_empresa_id ON beneficios.usuarios(empresa_id);
CREATE INDEX idx_usuarios_nome ON beneficios.usuarios(nome);
