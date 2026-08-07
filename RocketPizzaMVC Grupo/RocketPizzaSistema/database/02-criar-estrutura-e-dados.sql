USE RocketPizzaDB;
GO

IF OBJECT_ID(N'dbo.LogsSistema', N'U') IS NULL
CREATE TABLE dbo.LogsSistema (
    LogSistemaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LogsSistema PRIMARY KEY,
    CriadoEm DATETIME2 NOT NULL CONSTRAINT DF_LogsSistema_CriadoEm DEFAULT SYSUTCDATETIME(),
    UsuarioEmail NVARCHAR(160) NOT NULL,
    Acao NVARCHAR(80) NOT NULL,
    Detalhes NVARCHAR(500) NOT NULL
);
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
CREATE TABLE dbo.Usuarios (
    UsuarioId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
    Nome NVARCHAR(120) NOT NULL,
    Email NVARCHAR(160) NOT NULL CONSTRAINT UQ_Usuarios_Email UNIQUE,
    Telefone NVARCHAR(20) NOT NULL,
    Senha NVARCHAR(120) NOT NULL,
    Perfil NVARCHAR(20) NOT NULL CONSTRAINT CK_Usuarios_Perfil CHECK (Perfil IN (N'Administrador', N'Cliente'))
);
GO

IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
CREATE TABLE dbo.Clientes (
    ClienteId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
    UsuarioId INT NULL CONSTRAINT FK_Clientes_Usuarios REFERENCES dbo.Usuarios(UsuarioId),
    Nome NVARCHAR(120) NOT NULL,
    Email NVARCHAR(160) NOT NULL,
    Telefone NVARCHAR(20) NOT NULL,
    Endereco NVARCHAR(180) NOT NULL,
    Numero NVARCHAR(12) NOT NULL
);
GO

IF OBJECT_ID(N'dbo.Produtos', N'U') IS NULL
CREATE TABLE dbo.Produtos (
    ProdutoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Produtos PRIMARY KEY,
    Nome NVARCHAR(120) NOT NULL,
    Categoria NVARCHAR(60) NOT NULL,
    Preco DECIMAL(10,2) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_Produtos_Ativo DEFAULT 1
);
GO

IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NULL
CREATE TABLE dbo.Pedidos (
    PedidoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pedidos PRIMARY KEY,
    ClienteId INT NOT NULL CONSTRAINT FK_Pedidos_Clientes REFERENCES dbo.Clientes(ClienteId),
    CriadoEm DATETIME2 NOT NULL CONSTRAINT DF_Pedidos_CriadoEm DEFAULT SYSUTCDATETIME(),
    Status NVARCHAR(40) NOT NULL CONSTRAINT DF_Pedidos_Status DEFAULT N'Recebido',
    Total DECIMAL(10,2) NOT NULL CONSTRAINT DF_Pedidos_Total DEFAULT 0,
    Observacao NVARCHAR(500) NOT NULL CONSTRAINT DF_Pedidos_Observacao DEFAULT N''
);
GO

IF OBJECT_ID(N'dbo.PedidoItens', N'U') IS NULL
CREATE TABLE dbo.PedidoItens (
    PedidoItemId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PedidoItens PRIMARY KEY,
    PedidoId INT NOT NULL CONSTRAINT FK_PedidoItens_Pedidos REFERENCES dbo.Pedidos(PedidoId) ON DELETE CASCADE,
    ProdutoId INT NOT NULL CONSTRAINT FK_PedidoItens_Produtos REFERENCES dbo.Produtos(ProdutoId),
    Quantidade INT NOT NULL,
    PrecoUnitario DECIMAL(10,2) NOT NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Email = N'admin@rocketpizza.com')
INSERT INTO dbo.Usuarios (Nome, Email, Telefone, Senha, Perfil)
VALUES (N'Administrador Rocket', N'admin@rocketpizza.com', N'(11) 99999-9999', N'Admin@123', N'Administrador');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Email = N'cliente@rocketpizza.com')
BEGIN
    INSERT INTO dbo.Usuarios (Nome, Email, Telefone, Senha, Perfil)
    VALUES (N'Cliente Rocket', N'cliente@rocketpizza.com', N'(11) 98888-7777', N'Cliente@123', N'Cliente');

    INSERT INTO dbo.Clientes (UsuarioId, Nome, Email, Telefone, Endereco, Numero)
    SELECT UsuarioId, Nome, Email, Telefone, N'Rua das Pizzas', N'123'
    FROM dbo.Usuarios
    WHERE Email = N'cliente@rocketpizza.com';
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Produtos)
INSERT INTO dbo.Produtos (Nome, Categoria, Preco, Ativo) VALUES
(N'Marguerita', N'Pizza', 79.90, 1),
(N'Calabresa', N'Pizza', 79.90, 1),
(N'Frango com Catupiry', N'Pizza', 79.90, 1),
(N'Combo Familia', N'Combo', 89.90, 1),
(N'Refrigerante Dois Litros', N'Bebida', 16.90, 1);
GO

