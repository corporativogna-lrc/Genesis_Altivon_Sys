CREATE DATABASE IF NOT EXISTS GenesisAltivonDb
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

CREATE USER IF NOT EXISTS 'altivon_usr'@'localhost' IDENTIFIED BY 'AltivonPassword123!';
CREATE USER IF NOT EXISTS 'altivon_usr'@'127.0.0.1' IDENTIFIED BY 'AltivonPassword123!';

GRANT ALL PRIVILEGES ON GenesisAltivonDb.* TO 'altivon_usr'@'localhost';
GRANT ALL PRIVILEGES ON GenesisAltivonDb.* TO 'altivon_usr'@'127.0.0.1';
FLUSH PRIVILEGES;

USE GenesisAltivonDb;

CREATE TABLE IF NOT EXISTS sys_tenants (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    razao_social VARCHAR(200) NOT NULL,
    nome_fantasia VARCHAR(200) NOT NULL,
    cnpj VARCHAR(18) NOT NULL,
    regime_tributario TINYINT UNSIGNED NOT NULL DEFAULT 1,
    ativo TINYINT(1) NOT NULL DEFAULT 1,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_at TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by VARCHAR(100) NOT NULL DEFAULT 'SYSTEM_BOOTSTRAP',
    updated_by VARCHAR(100) NOT NULL DEFAULT 'SYSTEM_BOOTSTRAP',
    row_version BIGINT UNSIGNED NOT NULL DEFAULT 1,
    PRIMARY KEY (id),
    UNIQUE KEY uk_sys_tenants_cnpj (cnpj),
    KEY idx_sys_tenants_tenant_id (tenant_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS sys_usuarios (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    nome_completo VARCHAR(180) NOT NULL,
    email VARCHAR(180) NOT NULL,
    senha_hash VARCHAR(255) NOT NULL,
    perfil_rbac VARCHAR(60) NOT NULL DEFAULT 'OPERADOR',
    mfa_habilitado TINYINT(1) NOT NULL DEFAULT 0,
    mfa_segredo_cifrado VARBINARY(512) NULL,
    refresh_token_hash CHAR(64) NULL,
    refresh_token_expira_em DATETIME(6) NULL,
    ativo TINYINT(1) NOT NULL DEFAULT 1,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_at TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by VARCHAR(100) NOT NULL DEFAULT 'SYSTEM_BOOTSTRAP',
    updated_by VARCHAR(100) NOT NULL DEFAULT 'SYSTEM_BOOTSTRAP',
    row_version BIGINT UNSIGNED NOT NULL DEFAULT 1,
    PRIMARY KEY (id),
    UNIQUE KEY uk_sys_usuarios_tenant_email (tenant_id, email),
    KEY idx_sys_usuarios_tenant_id (tenant_id),
    CONSTRAINT fk_sys_usuarios_tenant FOREIGN KEY (tenant_id) REFERENCES sys_tenants (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS sys_audit_logs (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    tenant_id CHAR(36) NOT NULL,
    entidade VARCHAR(120) NOT NULL,
    entidade_id VARCHAR(64) NOT NULL,
    acao VARCHAR(30) NOT NULL,
    usuario_id VARCHAR(100) NOT NULL,
    ip_origem VARCHAR(45) NULL,
    dados_anteriores LONGTEXT NULL,
    dados_novos LONGTEXT NULL,
    registrado_em TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY idx_sys_audit_tenant_entidade (tenant_id, entidade, entidade_id),
    KEY idx_sys_audit_data (registrado_em)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT IGNORE INTO sys_tenants (
    id,
    tenant_id,
    razao_social,
    nome_fantasia,
    cnpj,
    regime_tributario,
    ativo,
    is_deleted,
    created_by,
    updated_by
) VALUES (
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001',
    'GRUPO NEXUM ALTIVON LTDA ME',
    'Genesis Altivon Matriz',
    '00.000.000/0001-00',
    1,
    1,
    0,
    'SYSTEM_BOOTSTRAP',
    'SYSTEM_BOOTSTRAP'
);