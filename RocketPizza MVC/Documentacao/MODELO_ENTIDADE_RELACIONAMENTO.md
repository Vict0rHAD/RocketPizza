# Modelo Entidade-Relacionamento — Rocket Pizza

```mermaid
erDiagram
    PERFIS ||--o{ CLIENTES : define
    CLIENTES ||--o{ ENDERECOS : possui
    CLIENTES ||--o{ PEDIDOS : realiza
    CLIENTES ||--o{ CONTATOS : envia
    ENDERECOS ||--o{ PEDIDOS : entrega
    CATEGORIAS ||--o{ SABORES : classifica
    PEDIDOS ||--|{ ITENS_PEDIDO : contem
    PRODUTOS ||--o{ ITENS_PEDIDO : referencia
    ITENS_PEDIDO ||--o{ ITEM_PEDIDO_SABORES : combina
    SABORES ||--o{ ITEM_PEDIDO_SABORES : compoe

    PERFIS { tinyint PerfilId PK string Nome }
    CLIENTES { int ClienteId PK tinyint PerfilId FK string Usuario string Email binary SenhaHash bool Ativo }
    ENDERECOS { int EnderecoId PK int ClienteId FK string Logradouro string CEP bool Principal }
    CATEGORIAS { int CategoriaId PK string Nome }
    SABORES { int SaborId PK int CategoriaId FK string Nome decimal PrecoBase bool Ativo }
    PRODUTOS { int ProdutoId PK string Nome string Tipo decimal Preco int Estoque }
    PEDIDOS { int PedidoId PK int ClienteId FK int EnderecoId FK string Status decimal Total datetime CriadoEm }
    ITENS_PEDIDO { int ItemPedidoId PK int PedidoId FK int ProdutoId FK int Quantidade decimal PrecoUnitario }
    ITEM_PEDIDO_SABORES { int ItemPedidoId PK_FK int SaborId PK_FK }
    CONTATOS { int ContatoId PK int ClienteId FK string Assunto string Status }
```

## Decisões principais

- Senhas são armazenadas como hash SHA-512 com salt individual; nunca em texto puro.
- Exclusões de clientes e sabores com histórico viram desativação lógica, preservando os pedidos.
- `ItensPedido` aceita produtos adicionais; sabores da pizza ficam em uma tabela associativa, permitindo meio a meio.
- O total do pedido é calculado pelo SQL Server para evitar divergências.
- Índices favorecem histórico por cliente e consulta do cardápio ativo.
