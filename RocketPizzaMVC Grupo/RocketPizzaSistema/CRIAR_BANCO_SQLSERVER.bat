@echo off
cd /d "%~dp0"
set "SERVIDOR=DESKTOP-8O1L7SM\Eduardo"
echo Criando banco RocketPizzaDB no servidor %SERVIDOR%...

where sqlcmd >nul 2>nul
if errorlevel 1 (
    echo ERRO: sqlcmd nao foi encontrado.
    echo Instale SQL Server Command Line Utilities ou execute os scripts da pasta database no SQL Server Management Studio.
    pause
    exit /b 1
)

sqlcmd -S "%SERVIDOR%" -E -i "database\01-criar-banco.sql"
if errorlevel 1 (
    echo Nao foi possivel criar o banco usando o servidor %SERVIDOR%.
    echo Verifique se o nome do servidor esta igual ao Visual Studio e se seu usuario tem permissao.
    pause
    exit /b 1
)

sqlcmd -S "%SERVIDOR%" -E -d RocketPizzaDB -i "database\02-criar-estrutura-e-dados.sql"
if errorlevel 1 (
    echo Nao foi possivel criar a estrutura e os dados no banco RocketPizzaDB.
    pause
    exit /b 1
)

echo Banco criado com sucesso.
pause
