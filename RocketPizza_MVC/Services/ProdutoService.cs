using RocketPizza.Models;
using RocketPizza.Repositories;
namespace RocketPizza.Services;
public sealed class ProdutoService(IProdutoRepository repository)
{
    public IReadOnlyList<Produto> Listar(string? pesquisa=null,int? categoriaId=null,bool ativos=true)=>
        repository.Listar(ativos).Where(x=>(string.IsNullOrWhiteSpace(pesquisa)||x.Nome.Contains(pesquisa,StringComparison.OrdinalIgnoreCase)||x.Descricao.Contains(pesquisa,StringComparison.OrdinalIgnoreCase))&&(!categoriaId.HasValue||x.CategoriaId==categoriaId)).ToList();
    public Produto? Obter(int id)=>repository.Obter(id);
    public IReadOnlyList<Categoria> Categorias()=>repository.Categorias();
    public void Salvar(ProdutoFormViewModel model)=>repository.Salvar(model);
    public void Excluir(int id)=>repository.Excluir(id);
}
