using Microsoft.Data.SqlClient;
using RocketPizza.Data;
using RocketPizza.Models;

namespace RocketPizza.Repositories;
public sealed class ProdutoRepository(AppDbContext context) : IProdutoRepository
{
    public IReadOnlyList<Produto> Listar(bool somenteAtivos=true)
    {
        using var cn=context.CreateConnection();cn.Open();
        var sql="SELECT s.SaborId,s.CategoriaId,c.Nome,s.Nome,s.Descricao,s.PrecoBase,s.Imagem,s.Ativo FROM dbo.Sabores s JOIN dbo.Categorias c ON c.CategoriaId=s.CategoriaId"+(somenteAtivos?" WHERE s.Ativo=1":"")+" ORDER BY c.Nome,s.Nome";
        using var cmd=new SqlCommand(sql,cn);using var r=cmd.ExecuteReader();var lista=new List<Produto>();
        while(r.Read())
        {
            var categoria=r.GetString(2);
            var pasta=categoria.Equals("Doce",StringComparison.OrdinalIgnoreCase)?"sobremesas":"pizzas";
            lista.Add(new(){ProdutoId=r.GetInt32(0),CategoriaId=r.GetInt32(1),Categoria=categoria,Nome=r.GetString(3),Descricao=r.GetString(4),Preco=r.GetDecimal(5),Imagem=r.IsDBNull(6)?null:$"img/{pasta}/{Path.GetFileName(r.GetString(6))}",Ativo=r.GetBoolean(7)});
        }
        return lista;
    }
    public Produto? Obter(int id)=>Listar(false).FirstOrDefault(x=>x.ProdutoId==id);
    public IReadOnlyList<Categoria> Categorias()
    {
        using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("SELECT CategoriaId,Nome,Ativo FROM dbo.Categorias ORDER BY Nome",cn);
        using var r=cmd.ExecuteReader();var lista=new List<Categoria>();while(r.Read())lista.Add(new(){CategoriaId=r.GetInt32(0),Nome=r.GetString(1),Ativo=r.GetBoolean(2)});return lista;
    }
    public void Salvar(ProdutoFormViewModel p)
    {
        using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("dbo.sp_SaborSalvar",cn){CommandType=System.Data.CommandType.StoredProcedure};
        cmd.Parameters.AddWithValue("@SaborId",(object?)p.ProdutoId??DBNull.Value);cmd.Parameters.AddWithValue("@CategoriaId",p.CategoriaId);
        cmd.Parameters.AddWithValue("@Nome",p.Nome.Trim());cmd.Parameters.AddWithValue("@Descricao",p.Descricao.Trim());
        cmd.Parameters.AddWithValue("@PrecoBase",p.Preco);cmd.Parameters.AddWithValue("@Ativo",p.Ativo);cmd.ExecuteNonQuery();
    }
    public void Excluir(int id)
    {
        using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("dbo.sp_SaborExcluir",cn){CommandType=System.Data.CommandType.StoredProcedure};
        cmd.Parameters.AddWithValue("@SaborId",id);cmd.ExecuteNonQuery();
    }
}
