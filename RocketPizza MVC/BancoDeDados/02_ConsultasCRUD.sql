USE RocketPizzaDB;
GO
-- Exemplos para testar no SSMS 21 (execute cada bloco conforme necessário).
EXEC dbo.sp_ClienteInserir N'Maria da Silva','maria@exemplo.com','11988887777','2000-01-01',N'Pizza@123';
SELECT ClienteId,Nome,Email,Telefone,Ativo,CriadoEm FROM dbo.Clientes ORDER BY ClienteId;
EXEC dbo.sp_ClienteAtualizar @ClienteId=2,@Nome=N'Maria Silva',@Email='maria@exemplo.com',@Telefone='11977776666',@Ativo=1;
-- EXEC dbo.sp_ClienteExcluir @ClienteId=2;

EXEC dbo.sp_SaborSalvar @CategoriaId=2,@Nome=N'Rocket Especial',@Descricao=N'Mussarela, pepperoni e manjericão',@PrecoBase=79.90;
SELECT s.*,c.Nome Categoria FROM dbo.Sabores s JOIN dbo.Categorias c ON c.CategoriaId=s.CategoriaId ORDER BY s.Nome;
-- EXEC dbo.sp_SaborExcluir @SaborId=7;

SELECT * FROM dbo.vw_PedidosResumo ORDER BY CriadoEm DESC;
-- EXEC dbo.sp_PedidoAtualizarStatus @PedidoId=1,@Status='Preparando';
