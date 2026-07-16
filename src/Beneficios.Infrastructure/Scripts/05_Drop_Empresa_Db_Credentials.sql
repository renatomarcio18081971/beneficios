-- Remove colunas de credenciais de banco por tenant (migração de ambientes existentes).
ALTER TABLE beneficios.empresas DROP COLUMN IF EXISTS nome_banco;
ALTER TABLE beneficios.empresas DROP COLUMN IF EXISTS usuario_banco;
ALTER TABLE beneficios.empresas DROP COLUMN IF EXISTS senha_banco;
