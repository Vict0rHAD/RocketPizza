# RocketPizza — ASP.NET Core MVC

Aplicação convertida integralmente para **ASP.NET Core MVC**, com site público,
área do cliente, pedidos e painel administrativo em um único projeto executável
no Visual Studio.

## Pré-requisitos

- Visual Studio com a carga de trabalho **ASP.NET e desenvolvimento Web**
- SDK do .NET 10
- SQL Server 2019 ou superior (a configuração padrão usa `.\SQLEXPRESS`)
- SQL Server Management Studio

## Preparação

1. No SSMS, conecte-se ao SQL Server e execute `BancoDeDados/01_CriarBanco.sql`.
2. Se a instância não for `SQLEXPRESS`, altere `ConnectionStrings:RocketPizza`
   em `appsettings.json`.
3. Abra `RocketPizza.slnx` no Visual Studio.
4. Confirme `RocketPizza.Web` como projeto de inicialização.
5. Pressione **F5** ou **Ctrl+F5**.

O NuGet restaura automaticamente `Microsoft.Data.SqlClient`.

## Acesso administrativo

- Usuário: `admin`
- Senha: `admin`

Essas credenciais são apenas para o cenário escolar.

## Estrutura MVC em camadas

- `Controllers/`: oito controladores separados por recurso
- `Models/`: Cliente, Produto, Categoria, Pedido, ItemPedido, Pagamento,
  Endereco, Unidade e Contato
- `Views/`: páginas Razor organizadas pelo nome de cada controlador
- `Data/`: contexto de conexão e inicialização
- `Repositories/`: contratos e persistência parametrizada no SQL Server
- `Services/`: regras de produtos, pedidos, clientes, contato e pagamento
- `wwwroot/`: CSS, JavaScript e imagens categorizadas

Fluxo principal: `Controller → Service → Repository → AppDbContext → SQL Server`.

## Funcionalidades

- Cadastro, login, logout e sessão de usuário
- Perfil do cliente
- Cardápio dinâmico vindo do banco
- Pedido com um ou dois sabores, tamanho, quantidade e pagamento
- Histórico de pedidos
- Formulário de contato persistido
- Administração de pedidos, clientes e sabores
- Validação no navegador e no servidor
- Proteção antifalsificação nos formulários
- Comandos SQL parametrizados e transação na criação de pedidos
