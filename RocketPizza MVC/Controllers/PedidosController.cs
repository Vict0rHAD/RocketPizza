using Microsoft.AspNetCore.Mvc;
using RocketPizza.Extensions;
using RocketPizza.Models;
using RocketPizza.Services;
namespace RocketPizza.Controllers;
public sealed class PedidosController(PedidoService pedidos,ProdutoService produtos):Controller
{
    public IActionResult Index()=>RedirectToAction(nameof(Criar));
    [HttpGet]public IActionResult Criar(){if(HttpContext.Session.GetUsuario() is null)return RedirectToAction("Login","Clientes",new{retorno=Url.Action(nameof(Criar))});ViewBag.Produtos=produtos.Listar();return View(new CriarPedidoViewModel());}
    [HttpPost,ValidateAntiForgeryToken]public IActionResult Criar(CriarPedidoViewModel model){var u=HttpContext.Session.GetUsuario();if(u is null)return RedirectToAction("Login","Clientes");if(!ModelState.IsValid){ViewBag.Produtos=produtos.Listar();return View(model);}try{var id=pedidos.Criar(u.ClienteId,model);return RedirectToAction(nameof(Confirmacao),new{id});}catch(Exception ex){ModelState.AddModelError("",ex.Message);ViewBag.Produtos=produtos.Listar();return View(model);}}
    public IActionResult Confirmacao(int id)=>View(id);
    public IActionResult MeusPedidos(){var u=HttpContext.Session.GetUsuario();return u is null?RedirectToAction("Login","Clientes",new{retorno=Url.Action(nameof(MeusPedidos))}):View(pedidos.Listar(u.ClienteId));}
}
