# Rocket Pizza Sistema

Solucao criada a partir do site Rocket Pizza com dois projetos usando SQL Server:

- `RocketPizza.Mvc`: ASP.NET Core MVC com login, cadastro, area do cliente e painel administrativo.
- `RocketPizza.Forms`: Windows Forms com login, cadastro, area do cliente e painel administrativo.
- `RocketPizza.Dados`: biblioteca compartilhada com modelos, validacoes, CRUD e logs.

## O que foi simplificado

Foram removidas as partes que mais travavam o projeto no Visual Studio:

- O MVC nao cria mais o banco automaticamente ao iniciar.
- O Forms nao cria mais o banco automaticamente ao iniciar.
- A conexao usa somente a connection string configurada.
- A referencia direta para DLL do `Microsoft.Data.SqlClient` foi trocada por `PackageReference`.
- O MVC usa chaves locais em `RocketPizza.Mvc/App_Data/Keys`, evitando erro de DataProtection/EventLog.

## Banco de dados

Banco usado: `RocketPizzaDB`

Connection string:

```text
Server=DESKTOP-8O1L7SM\Eduardo;Database=RocketPizzaDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False
```

Scripts:

- `database/01-criar-banco.sql`
- `database/02-criar-estrutura-e-dados.sql`

Para criar o banco, execute:

```text
CRIAR_BANCO_SQLSERVER.bat
```

Depois abra o MVC ou o Forms.

## Acessos iniciais

- Administrador: `admin@rocketpizza.com` / `Admin@123`
- Cliente: `cliente@rocketpizza.com` / `Cliente@123`

## Cadastro individual

O cliente tambem pode criar o proprio acesso:

- MVC: menu `Cadastro` ou botao `Criar meu cadastro` no login.
- Forms: botao `Criar cadastro` no login.

Ao cadastrar, o sistema grava:

- Um registro em `Usuarios` com perfil `Cliente`.
- Um registro em `Clientes` ligado ao usuario.
- Um log de cadastro em `LogsSistema`.

## Funcionalidades

- Login com log gravado em `LogsSistema`.
- Validacao em portugues para nome, telefone e numero.
- Telefone no padrao `(11) 99999-9999`.
- Campo errado fica vermelho; campo certo fica verde.
- Campo numerico bloqueia letras.
- Campos de letras rejeitam numeros.
- Admin: CRUD de usuarios, clientes, produtos e pedidos.
- Cliente: cria pedido e consulta `Meus pedidos`.

## Como rodar

Mais facil:

- Execute `CRIAR_BANCO_SQLSERVER.bat`.
- Abra `INICIAR_MVC.bat` para iniciar o site MVC.
- Abra `INICIAR_FORMS.bat` para iniciar o Windows Forms.

No Visual Studio:

- Clique com o botao direito em `RocketPizza.Mvc` ou `RocketPizza.Forms`.
- Escolha `Definir como Projeto de Inicializacao`.

Nao inicie `RocketPizza.Dados`: ele e uma biblioteca de classes usada pelos outros projetos.
