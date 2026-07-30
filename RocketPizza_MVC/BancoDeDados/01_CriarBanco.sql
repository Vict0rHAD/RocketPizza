/* Rocket Pizza - SQL Server 2019+ / SSMS 21. Execute conectado ao servidor local. */
USE master;
GO
IF DB_ID(N'RocketPizzaDB') IS NULL CREATE DATABASE RocketPizzaDB;
GO
USE RocketPizzaDB;
GO

CREATE TABLE dbo.Perfis (
    PerfilId TINYINT IDENTITY PRIMARY KEY,
    Nome VARCHAR(30) NOT NULL UNIQUE CHECK (Nome IN ('Administrador','Cliente'))
);
CREATE TABLE dbo.Clientes (
    ClienteId INT IDENTITY PRIMARY KEY,
    PerfilId TINYINT NOT NULL CONSTRAINT FK_Clientes_Perfis REFERENCES dbo.Perfis(PerfilId),
    Nome NVARCHAR(120) NOT NULL CHECK (LEN(LTRIM(RTRIM(Nome))) >= 3),
    Usuario VARCHAR(50) NULL UNIQUE,
    Email VARCHAR(254) NOT NULL UNIQUE CHECK (Email LIKE '%_@_%._%'),
    Telefone VARCHAR(15) NOT NULL,
    DataNascimento DATE NULL CHECK (DataNascimento IS NULL OR DataNascimento <= CAST(GETDATE() AS DATE)),
    SenhaHash VARBINARY(64) NOT NULL,
    SenhaSalt UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Ativo BIT NOT NULL DEFAULT 1,
    CriadoEm DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    AtualizadoEm DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE TABLE dbo.Enderecos (
    EnderecoId INT IDENTITY PRIMARY KEY,
    ClienteId INT NOT NULL CONSTRAINT FK_Enderecos_Clientes REFERENCES dbo.Clientes(ClienteId) ON DELETE CASCADE,
    Logradouro NVARCHAR(150) NOT NULL, Numero VARCHAR(10) NOT NULL,
    Complemento NVARCHAR(80) NULL, Bairro NVARCHAR(80) NOT NULL,
    Cidade NVARCHAR(80) NOT NULL, UF CHAR(2) NOT NULL, CEP CHAR(8) NOT NULL,
    Principal BIT NOT NULL DEFAULT 0
);
CREATE UNIQUE INDEX UX_Endereco_Principal ON dbo.Enderecos(ClienteId) WHERE Principal = 1;
CREATE TABLE dbo.Categorias (
    CategoriaId INT IDENTITY PRIMARY KEY, Nome NVARCHAR(60) NOT NULL UNIQUE, Ativo BIT NOT NULL DEFAULT 1
);
CREATE TABLE dbo.Sabores (
    SaborId INT IDENTITY PRIMARY KEY,
    CategoriaId INT NOT NULL CONSTRAINT FK_Sabores_Categorias REFERENCES dbo.Categorias(CategoriaId),
    Nome NVARCHAR(100) NOT NULL UNIQUE, Descricao NVARCHAR(300) NOT NULL,
    PrecoBase DECIMAL(10,2) NOT NULL CHECK (PrecoBase > 0), Imagem VARCHAR(200) NULL,
    Ativo BIT NOT NULL DEFAULT 1, CriadoEm DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE TABLE dbo.Produtos (
    ProdutoId INT IDENTITY PRIMARY KEY, Nome NVARCHAR(100) NOT NULL UNIQUE,
    Tipo VARCHAR(20) NOT NULL CHECK (Tipo IN ('Entrada','Bebida','Sobremesa')),
    Preco DECIMAL(10,2) NOT NULL CHECK (Preco >= 0), Estoque INT NOT NULL DEFAULT 0 CHECK (Estoque >= 0), Ativo BIT NOT NULL DEFAULT 1
);
CREATE TABLE dbo.Pedidos (
    PedidoId INT IDENTITY PRIMARY KEY, ClienteId INT NOT NULL CONSTRAINT FK_Pedidos_Clientes REFERENCES dbo.Clientes(ClienteId),
    EnderecoId INT NULL CONSTRAINT FK_Pedidos_Enderecos REFERENCES dbo.Enderecos(EnderecoId),
    Status VARCHAR(20) NOT NULL DEFAULT 'Recebido' CHECK (Status IN ('Recebido','Preparando','SaiuEntrega','Concluido','Cancelado')),
    FormaPagamento VARCHAR(20) NOT NULL CHECK (FormaPagamento IN ('Pix','Cartao','Dinheiro')),
    Observacoes NVARCHAR(500) NULL, Subtotal DECIMAL(10,2) NOT NULL CHECK (Subtotal >= 0),
    TaxaEntrega DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK (TaxaEntrega >= 0),
    Total AS (Subtotal + TaxaEntrega) PERSISTED, CriadoEm DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE TABLE dbo.ItensPedido (
    ItemPedidoId INT IDENTITY PRIMARY KEY, PedidoId INT NOT NULL CONSTRAINT FK_ItensPedido_Pedidos REFERENCES dbo.Pedidos(PedidoId) ON DELETE CASCADE,
    ProdutoId INT NULL CONSTRAINT FK_ItensPedido_Produtos REFERENCES dbo.Produtos(ProdutoId),
    Tamanho VARCHAR(15) NULL CHECK (Tamanho IS NULL OR Tamanho IN ('Pequena','Media','Grande','Familia')),
    Quantidade SMALLINT NOT NULL CHECK (Quantidade BETWEEN 1 AND 99), PrecoUnitario DECIMAL(10,2) NOT NULL CHECK (PrecoUnitario >= 0)
);
CREATE TABLE dbo.ItemPedidoSabores (
    ItemPedidoId INT NOT NULL CONSTRAINT FK_ItemSabores_Item REFERENCES dbo.ItensPedido(ItemPedidoId) ON DELETE CASCADE,
    SaborId INT NOT NULL CONSTRAINT FK_ItemSabores_Sabor REFERENCES dbo.Sabores(SaborId),
    CONSTRAINT PK_ItemPedidoSabores PRIMARY KEY(ItemPedidoId,SaborId)
);
CREATE TABLE dbo.Contatos (
    ContatoId INT IDENTITY PRIMARY KEY, ClienteId INT NULL CONSTRAINT FK_Contatos_Clientes REFERENCES dbo.Clientes(ClienteId),
    Nome NVARCHAR(120) NOT NULL, Email VARCHAR(254) NOT NULL, Telefone VARCHAR(15) NULL,
    Assunto NVARCHAR(80) NOT NULL, Mensagem NVARCHAR(500) NOT NULL,
    Status VARCHAR(15) NOT NULL DEFAULT 'Aberto' CHECK(Status IN ('Aberto','Respondido','Arquivado')), CriadoEm DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE INDEX IX_Pedidos_Cliente_Data ON dbo.Pedidos(ClienteId,CriadoEm DESC);
CREATE INDEX IX_Sabores_Ativos ON dbo.Sabores(Ativo,Nome);
GO

INSERT dbo.Perfis(Nome) VALUES ('Administrador'),('Cliente');
INSERT dbo.Categorias(Nome) VALUES (N'Tradicional'),(N'Especial'),(N'Doce');
INSERT dbo.Sabores(CategoriaId,Nome,Descricao,PrecoBase,Imagem) VALUES
(1,N'Mussarela',N'Molho, mussarela e orégano',59.90,'img/mussarela.png'),
(1,N'Calabresa',N'Calabresa, cebola e azeitonas',64.90,'img/calabresa.png'),
(1,N'Marguerita',N'Mussarela, tomate e manjericão',64.90,'img/marguerita.png'),
(2,N'Frango com Catupiry',N'Frango desfiado e Catupiry',72.90,'img/frango.png'),
(2,N'Quatro Queijos',N'Mussarela, provolone, parmesão e catupiry',74.90,'img/quatro-queijos.png'),
(3,N'Chocolate',N'Chocolate ao leite e granulado',69.90,'img/chocolate.jpg');
INSERT dbo.Produtos(Nome,Tipo,Preco,Estoque) VALUES
(N'Bolinhas de queijo','Entrada',18.90,50),(N'Anéis de cebola','Entrada',19.90,50),
(N'Refrigerante 2L','Bebida',14.00,100),(N'Água 500ml','Bebida',5.00,100);

/* Credenciais escolares solicitadas: admin / admin. Troque a senha no primeiro uso. */
DECLARE @Salt UNIQUEIDENTIFIER=NEWID();
INSERT dbo.Clientes(PerfilId,Nome,Usuario,Email,Telefone,SenhaSalt,SenhaHash)
SELECT PerfilId,N'Administrador', 'admin', 'admin@rocketpizza.local', '11999999999', @Salt,
HASHBYTES('SHA2_512',CONVERT(VARBINARY(36),@Salt)+CONVERT(VARBINARY(4000),N'admin'))
FROM dbo.Perfis WHERE Nome='Administrador';
GO

CREATE OR ALTER PROCEDURE dbo.sp_Autenticar @Login VARCHAR(254), @Senha NVARCHAR(72) AS
BEGIN SET NOCOUNT ON;
 SELECT c.ClienteId,c.Nome,c.Email,p.Nome Perfil
 FROM dbo.Clientes c JOIN dbo.Perfis p ON p.PerfilId=c.PerfilId
 WHERE c.Ativo=1 AND (c.Email=LOWER(LTRIM(RTRIM(@Login))) OR c.Usuario=LOWER(LTRIM(RTRIM(@Login))))
 AND c.SenhaHash=HASHBYTES('SHA2_512',CONVERT(VARBINARY(36),c.SenhaSalt)+CONVERT(VARBINARY(4000),@Senha));
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_ClienteInserir @Nome NVARCHAR(120),@Email VARCHAR(254),@Telefone VARCHAR(15),@DataNascimento DATE,@Senha NVARCHAR(72) AS
BEGIN SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @Senha NOT LIKE '%[A-Z]%' OR @Senha NOT LIKE '%[a-z]%' OR @Senha NOT LIKE '%[0-9]%' OR LEN(@Senha)<8 THROW 50001,'Senha fora da política.',1;
 DECLARE @Salt UNIQUEIDENTIFIER=NEWID();
 IF @DataNascimento > DATEADD(YEAR,-13,CAST(GETDATE() AS DATE)) THROW 50002,'É necessário ter pelo menos 13 anos.',1;
 INSERT dbo.Clientes(PerfilId,Nome,Email,Telefone,DataNascimento,SenhaSalt,SenhaHash)
 SELECT PerfilId,LTRIM(RTRIM(@Nome)),LOWER(LTRIM(RTRIM(@Email))),@Telefone,@DataNascimento,@Salt,HASHBYTES('SHA2_512',CONVERT(VARBINARY(36),@Salt)+CONVERT(VARBINARY(4000),@Senha)) FROM dbo.Perfis WHERE Nome='Cliente';
 SELECT CAST(SCOPE_IDENTITY() AS INT) ClienteId;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_ClienteAtualizar @ClienteId INT,@Nome NVARCHAR(120),@Email VARCHAR(254),@Telefone VARCHAR(15),@Ativo BIT AS
 UPDATE dbo.Clientes SET Nome=LTRIM(RTRIM(@Nome)),Email=LOWER(LTRIM(RTRIM(@Email))),Telefone=@Telefone,Ativo=@Ativo,AtualizadoEm=SYSDATETIME() WHERE ClienteId=@ClienteId;
GO
CREATE OR ALTER PROCEDURE dbo.sp_ClienteExcluir @ClienteId INT AS
BEGIN
 IF EXISTS(SELECT 1 FROM dbo.Pedidos WHERE ClienteId=@ClienteId) UPDATE dbo.Clientes SET Ativo=0,AtualizadoEm=SYSDATETIME() WHERE ClienteId=@ClienteId;
 ELSE DELETE dbo.Clientes WHERE ClienteId=@ClienteId AND Email<>'admin';
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_SaborSalvar @SaborId INT=NULL,@CategoriaId INT,@Nome NVARCHAR(100),@Descricao NVARCHAR(300),@PrecoBase DECIMAL(10,2),@Ativo BIT=1 AS
BEGIN
 IF @SaborId IS NULL INSERT dbo.Sabores(CategoriaId,Nome,Descricao,PrecoBase,Ativo) VALUES(@CategoriaId,LTRIM(RTRIM(@Nome)),@Descricao,@PrecoBase,@Ativo);
 ELSE UPDATE dbo.Sabores SET CategoriaId=@CategoriaId,Nome=LTRIM(RTRIM(@Nome)),Descricao=@Descricao,PrecoBase=@PrecoBase,Ativo=@Ativo WHERE SaborId=@SaborId;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_SaborExcluir @SaborId INT AS
BEGIN
 IF EXISTS(SELECT 1 FROM dbo.ItemPedidoSabores WHERE SaborId=@SaborId) UPDATE dbo.Sabores SET Ativo=0 WHERE SaborId=@SaborId;
 ELSE DELETE dbo.Sabores WHERE SaborId=@SaborId;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_PedidoAtualizarStatus @PedidoId INT,@Status VARCHAR(20) AS
 UPDATE dbo.Pedidos SET Status=@Status WHERE PedidoId=@PedidoId;
GO
CREATE OR ALTER VIEW dbo.vw_PedidosResumo AS
 SELECT p.PedidoId,c.Nome Cliente,p.Status,p.FormaPagamento,p.Subtotal,p.TaxaEntrega,p.Total,p.CriadoEm
 FROM dbo.Pedidos p JOIN dbo.Clientes c ON c.ClienteId=p.ClienteId;
GO
