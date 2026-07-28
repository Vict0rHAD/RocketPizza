using RocketPizza.Models;
namespace RocketPizza.Repositories;
public interface IPedidoRepository
{
    int Criar(int clienteId,CriarPedidoViewModel pedido,IReadOnlyList<Produto> produtos);
    IReadOnlyList<Pedido> Listar(int? clienteId=null);
    void AtualizarStatus(int id,string status);
}
