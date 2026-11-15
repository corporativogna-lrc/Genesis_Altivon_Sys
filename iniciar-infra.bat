@echo off
setlocal

cd /d "D:\Genesis_Altivon_Sys"
if errorlevel 1 goto erro_diretorio

if not exist "C:\xampp\mysql\bin\mysqld.exe" goto erro_mysqld
if not exist "D:\Genesis_Altivon_Sys\Infraestrutura\BancoDeDados\init_genesis_altivon_db.sql" goto erro_sql

if not exist "D:\Genesis_Altivon_Sys\StorageDados" mkdir "D:\Genesis_Altivon_Sys\StorageDados"
if not exist "D:\Genesis_Altivon_Sys\Logs" mkdir "D:\Genesis_Altivon_Sys\Logs"

netstat -ano | findstr ":3309" | findstr "LISTENING" >nul 2>&1
if not errorlevel 1 goto aplicar_schema

echo [INFO] Iniciando servidor MariaDB/MySQL nativo do XAMPP na porta 3309...
start "" /B "C:\xampp\mysql\bin\mysqld.exe" --defaults-file="C:\xampp\mysql\bin\my.ini" --standalone
set tentativas=0

:aguardar_mysql
set /a tentativas=tentativas+1
if %tentativas% gtr 15 goto erro_timeout
timeout /t 2 /nobreak >nul
netstat -ano | findstr ":3309" | findstr "LISTENING" >nul 2>&1
if errorlevel 1 (
    echo [INFO] Aguardando MariaDB escutar na porta 3309 - tentativa %tentativas% de 15...
    goto aguardar_mysql
)

:aplicar_schema
echo [OK] MariaDB ativo na porta 3309. Aplicando schema canonico GenesisAltivonDb...
"C:\xampp\mysql\bin\mysql.exe" -h 127.0.0.1 -P 3309 -u root < "D:\Genesis_Altivon_Sys\Infraestrutura\BancoDeDados\init_genesis_altivon_db.sql"
if errorlevel 1 goto erro_execucao_sql

echo [OK] Validando acesso do usuario altivon_usr e tabelas criadas...
"C:\xampp\mysql\bin\mysql.exe" -h 127.0.0.1 -P 3309 -u altivon_usr -pAltivonPassword123! -e "USE GenesisAltivonDb; SHOW TABLES; SELECT id, nome_fantasia FROM sys_tenants;"
if errorlevel 1 goto erro_validacao_usuario

echo [CONCLUIDO] Infraestrutura de Banco de Dados GenesisAltivonDb validada na porta 3309.
endlocal
exit /b 0

:erro_diretorio
echo [ERRO] Diretorio D:\Genesis_Altivon_Sys nao encontrado.
exit /b 1

:erro_mysqld
echo [ERRO] Binario C:\xampp\mysql\bin\mysqld.exe nao encontrado.
exit /b 1

:erro_sql
echo [ERRO] Script init_genesis_altivon_db.sql nao encontrado.
exit /b 1

:erro_timeout
echo [ERRO] O servidor MariaDB/MySQL nao abriu a porta 3309 apos 30 segundos.
if exist "C:\xampp\mysql\data\mysql_error.log" (
    powershell -NoProfile -Command "Get-Content 'C:\xampp\mysql\data\mysql_error.log' -Tail 25"
)
exit /b 1

:erro_execucao_sql
echo [ERRO] Falha ao executar init_genesis_altivon_db.sql na porta 3309.
exit /b 1

:erro_validacao_usuario
echo [ERRO] Falha na validacao de leitura com o usuario altivon_usr.
exit /b 1