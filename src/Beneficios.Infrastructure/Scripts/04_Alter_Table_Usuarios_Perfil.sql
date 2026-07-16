ALTER TABLE beneficios.usuarios
    ADD COLUMN perfil VARCHAR(20) NOT NULL DEFAULT 'Empresa';

UPDATE beneficios.usuarios SET perfil = 'Admin' WHERE email = 'admin@exemplo.com';
