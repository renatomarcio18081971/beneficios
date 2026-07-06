CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Inserir empresas e capturar os IDs gerados
WITH nova_empresa AS (
    INSERT INTO empresas (id, razao_social, dominio, nome_banco, usuario_banco, senha_banco, data_inclusao)
    VALUES 
        (gen_random_uuid(), 'Empresa Exemplo LTDA', 'exemplo', 
         'ZGJfZXhlbXBsbw==', 'dXNlcl9leGVtcGxv', 'cGFzc19leGVtcGxv', NOW()),
        (gen_random_uuid(), 'Outra Empresa SA', 'outra', 
         'ZGJfb3V0cmE=', 'dXNlcl9vdXRyYQ==', 'cGFzc19vdXRyYQ==', NOW())
    RETURNING id, dominio
)
-- Inserir usuários vinculados às empresas criadas
INSERT INTO usuarios (id, nome, senha, email, perfil, empresa_id, data_inclusao)
VALUES
    (gen_random_uuid(), 'Administrador', 'YWRtaW4xMjM=', 
     'admin@exemplo.com', 'Admin', (SELECT id FROM nova_empresa WHERE dominio = 'exemplo'), NOW()),
    (gen_random_uuid(), 'Usuário Teste', 'dGVzdGUxMjM=', 
     'teste@exemplo.com', 'Empresa', (SELECT id FROM nova_empresa WHERE dominio = 'exemplo'), NOW());
