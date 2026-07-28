namespace RocketPizza.Models;
public sealed class ItemPedido
{
    public int ItemPedidoId { get; set; }
    public int PedidoId { get; set; }
    public int? ProdutoId { get; set; }
    public string? Tamanho { get; set; }
    public short Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}
