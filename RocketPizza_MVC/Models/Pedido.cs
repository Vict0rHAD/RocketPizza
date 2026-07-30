using System.ComponentModel.DataAnnotations;

namespace RocketPizza.Models;
public sealed class Pedido
{
    public int PedidoId { get; set; }
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = "";
    public string Status { get; set; } = "";
    public string FormaPagamento { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal TaxaEntrega { get; set; }
    public decimal Total { get; set; }
    public DateTime CriadoEm { get; set; }
}

public sealed class CriarPedidoViewModel
{
    [Required] public string Tamanho { get; set; } = "Grande";
    [Range(1, int.MaxValue)] public int ProdutoId { get; set; }
    public int? SegundoProdutoId { get; set; }
    [Range(1, 20)] public int Quantidade { get; set; } = 1;
    [Required] public string FormaPagamento { get; set; } = "Pix";
    [StringLength(500)] public string? Observacoes { get; set; }
}
