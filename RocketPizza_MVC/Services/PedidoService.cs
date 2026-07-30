using RocketPizza.Models;
using RocketPizza.Repositories;
namespace RocketPizza.Services;
public sealed class PedidoService(IPedidoRepository pedidos,IProdutoRepository produtos,PagamentoService pagamentos)
{
    public int Criar(int clienteId,CriarPedidoViewModel model){pagamentos.Validar(model.FormaPagamento);var selecionados=produtos.Listar().Where(x=>x.ProdutoId==model.ProdutoId||x.ProdutoId==model.SegundoProdutoId).ToList();return pedidos.Criar(clienteId,model,selecionados);}
    public IReadOnlyList<Pedido> Listar(int? clienteId=null)=>pedidos.Listar(clienteId);
    public void AtualizarStatus(int id,string status)=>pedidos.AtualizarStatus(id,status);
}
