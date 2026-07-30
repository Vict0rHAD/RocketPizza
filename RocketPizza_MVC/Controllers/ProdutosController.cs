using Microsoft.AspNetCore.Mvc;
using RocketPizza.Services;
namespace RocketPizza.Controllers;
public sealed class ProdutosController(ProdutoService service):Controller
{
    public IActionResult Index()=>View(service.Listar());
    public IActionResult Detalhes(int id){var p=service.Obter(id);return p is null?NotFound():View(p);}
    public IActionResult Categoria(int id){ViewBag.Categoria=service.Categorias().FirstOrDefault(x=>x.CategoriaId==id)?.Nome;return View(service.Listar(categoriaId:id));}
    public IActionResult Pesquisa(string termo=""){ViewBag.Termo=termo;return View(service.Listar(termo));}
}
