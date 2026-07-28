using Microsoft.Data.SqlClient;
using RocketPizza.Data;
using RocketPizza.Models;

namespace RocketPizza.Repositories;
public sealed class PedidoRepository(AppDbContext context):IPedidoRepository
{
    public int Criar(int clienteId,CriarPedidoViewModel m,IReadOnlyList<Produto> produtos)
    {
        if(produtos.Count==0)throw new InvalidOperationException("Selecione um produto válido.");
        var unitario=produtos.Max(x=>x.Preco);var fator=m.Tamanho switch{"Pequena"=>.75m,"Media"=>.9m,"Familia"=>1.3m,_=>1m};
        using var cn=context.CreateConnection();cn.Open();using var tx=cn.BeginTransaction();
        using var cmd=new SqlCommand("INSERT dbo.Pedidos(ClienteId,Status,FormaPagamento,Observacoes,Subtotal,TaxaEntrega) OUTPUT INSERTED.PedidoId VALUES(@Cliente,'Recebido',@Forma,@Obs,@Subtotal,5)",cn,tx);
        cmd.Parameters.AddWithValue("@Cliente",clienteId);cmd.Parameters.AddWithValue("@Forma",m.FormaPagamento);cmd.Parameters.AddWithValue("@Obs",(object?)m.Observacoes??DBNull.Value);cmd.Parameters.AddWithValue("@Subtotal",decimal.Round(unitario*fator*m.Quantidade,2));
        var pedidoId=Convert.ToInt32(cmd.ExecuteScalar());
        using var item=new SqlCommand("INSERT dbo.ItensPedido(PedidoId,Tamanho,Quantidade,PrecoUnitario) OUTPUT INSERTED.ItemPedidoId VALUES(@Pedido,@Tamanho,@Qtd,@Preco)",cn,tx);
        item.Parameters.AddWithValue("@Pedido",pedidoId);item.Parameters.AddWithValue("@Tamanho",m.Tamanho);item.Parameters.AddWithValue("@Qtd",m.Quantidade);item.Parameters.AddWithValue("@Preco",unitario*fator);
        var itemId=Convert.ToInt32(item.ExecuteScalar());
        foreach(var produto in produtos){using var link=new SqlCommand("INSERT dbo.ItemPedidoSabores(ItemPedidoId,SaborId) VALUES(@Item,@Sabor)",cn,tx);link.Parameters.AddWithValue("@Item",itemId);link.Parameters.AddWithValue("@Sabor",produto.ProdutoId);link.ExecuteNonQuery();}
        tx.Commit();return pedidoId;
    }
    public IReadOnlyList<Pedido> Listar(int? clienteId=null)
    {
        using var cn=context.CreateConnection();cn.Open();var sql="SELECT v.PedidoId,p.ClienteId,v.Cliente,v.Status,v.FormaPagamento,v.Subtotal,v.TaxaEntrega,v.Total,v.CriadoEm FROM dbo.vw_PedidosResumo v JOIN dbo.Pedidos p ON p.PedidoId=v.PedidoId"+(clienteId.HasValue?" WHERE p.ClienteId=@Id":"")+" ORDER BY v.CriadoEm DESC";
        using var cmd=new SqlCommand(sql,cn);if(clienteId.HasValue)cmd.Parameters.AddWithValue("@Id",clienteId);using var r=cmd.ExecuteReader();var lista=new List<Pedido>();
        while(r.Read())lista.Add(new(){PedidoId=r.GetInt32(0),ClienteId=r.GetInt32(1),Cliente=r.GetString(2),Status=r.GetString(3),FormaPagamento=r.GetString(4),Subtotal=r.GetDecimal(5),TaxaEntrega=r.GetDecimal(6),Total=r.GetDecimal(7),CriadoEm=r.GetDateTime(8)});return lista;
    }
    public void AtualizarStatus(int id,string status)
    {
        if(!new[]{"Recebido","Preparando","SaiuEntrega","Concluido","Cancelado"}.Contains(status))throw new InvalidOperationException("Status inválido.");
        using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("dbo.sp_PedidoAtualizarStatus",cn){CommandType=System.Data.CommandType.StoredProcedure};cmd.Parameters.AddWithValue("@PedidoId",id);cmd.Parameters.AddWithValue("@Status",status);cmd.ExecuteNonQuery();
    }
}
