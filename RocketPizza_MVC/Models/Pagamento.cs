namespace RocketPizza.Models;
public sealed class Pagamento
{
    public int PagamentoId { get; set; }
    public int PedidoId { get; set; }
    public string Forma { get; set; } = "Pix";
    public decimal Valor { get; set; }
    public string Status { get; set; } = "Pendente";
}
