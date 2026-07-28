using Microsoft.AspNetCore.Mvc;
using RocketPizza.Extensions;
using RocketPizza.Models;
using RocketPizza.Services;
namespace RocketPizza.Controllers;
public sealed class AdministradorController(ProdutoService produtos,PedidoService pedidos,ClienteService clientes):Controller
{
    private bool Autorizado=>HttpContext.Session.GetUsuario()?.Perfil=="Administrador";
    private IActionResult? Bloqueio()=>Autorizado?null:RedirectToAction("Login","Clientes");
    public IActionResult Dashboard()=>Bloqueio()??View();
    public IActionResult Produtos()=>Bloqueio()??View(produtos.Listar(ativos:false));
    public IActionResult Pedidos()=>Bloqueio()??View(pedidos.Listar());
    public IActionResult Clientes()=>Bloqueio()??View(clientes.Listar());
    [HttpPost,ValidateAntiForgeryToken]public IActionResult SalvarProduto(ProdutoFormViewModel model){if(!Autorizado)return Unauthorized();if(ModelState.IsValid)produtos.Salvar(model);return RedirectToAction(nameof(Produtos));}
    [HttpPost,ValidateAntiForgeryToken]public IActionResult ExcluirProduto(int id){if(!Autorizado)return Unauthorized();produtos.Excluir(id);return RedirectToAction(nameof(Produtos));}
    [HttpPost,ValidateAntiForgeryToken]public IActionResult Status(int id,string status){if(!Autorizado)return Unauthorized();pedidos.AtualizarStatus(id,status);return RedirectToAction(nameof(Pedidos));}
    [HttpPost,ValidateAntiForgeryToken]public IActionResult AlternarCliente(int id){if(!Autorizado)return Unauthorized();clientes.AlternarAtivo(id);return RedirectToAction(nameof(Clientes));}
    [HttpPost,ValidateAntiForgeryToken]public IActionResult ExcluirCliente(int id){if(!Autorizado)return Unauthorized();clientes.Excluir(id);return RedirectToAction(nameof(Clientes));}
}
