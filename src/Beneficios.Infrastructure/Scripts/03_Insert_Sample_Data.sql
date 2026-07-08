CREATE EXTENSION IF NOT EXISTS "pgcrypto";

WITH nova_empresa AS (
    INSERT INTO empresas (id, razao_social, dominio, data_inclusao)
    VALUES
        (gen_random_uuid(), 'Empresa Exemplo LTDA', 'exemplo', NOW()),
        (gen_random_uuid(), 'Outra Empresa SA', 'outra', NOW())
    RETURNING id, dominio
)
INSERT INTO usuarios (id, nome, senha, email, perfil, empresa_id, data_inclusao)
VALUES
    (gen_random_uuid(), 'Administrador', 'YWRtaW4xMjM=', 'admin@exemplo.com', 'Admin', NULL, NOW());
