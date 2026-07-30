using RocketPizza.Models;
namespace RocketPizza.Repositories;
public interface IProdutoRepository
{
    IReadOnlyList<Produto> Listar(bool somenteAtivos=true);
    Produto? Obter(int id);
    IReadOnlyList<Categoria> Categorias();
    void Salvar(ProdutoFormViewModel produto);
    void Excluir(int id);
}
