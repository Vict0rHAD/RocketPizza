using Microsoft.AspNetCore.Mvc;
using RocketPizza.Extensions;
using RocketPizza.Models;
using RocketPizza.Services;
namespace RocketPizza.Controllers;
public sealed class ClientesController(ClienteService service):Controller
{
    [HttpGet]public IActionResult Login(string? retorno)=>View(new LoginViewModel{Retorno=retorno});
    [HttpPost,ValidateAntiForgeryToken]public IActionResult Login(LoginViewModel model){if(!ModelState.IsValid)return View(model);try{var u=service.Autenticar(model.Login,model.Senha);if(u is null){ModelState.AddModelError("","Credenciais inválidas ou usuário inativo.");return View(model);}HttpContext.Session.SetUsuario(u);if(u.Perfil=="Administrador")return RedirectToAction("Dashboard","Administrador");return LocalRedirect(Url.IsLocalUrl(model.Retorno)?model.Retorno!:Url.Action("Index","Home")!);}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(model);}}
    [HttpGet]public IActionResult Cadastro()=>View(new CadastroViewModel());
    [HttpPost,ValidateAntiForgeryToken]public IActionResult Cadastro(CadastroViewModel model){if(!ModelState.IsValid)return View(model);try{service.Cadastrar(model);TempData["Sucesso"]="Cadastro concluído.";return RedirectToAction(nameof(Login));}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(model);}}
    public IActionResult Perfil()=>RedirectToAction("Index","Perfil");
    [HttpPost,ValidateAntiForgeryToken]public IActionResult Sair(){HttpContext.Session.Clear();return RedirectToAction("Index","Home");}
}
