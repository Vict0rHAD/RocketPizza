using Microsoft.AspNetCore.Mvc;
using RocketPizza.Extensions;
using RocketPizza.Models;
using RocketPizza.Services;
namespace RocketPizza.Controllers;
public sealed class PerfilController(ClienteService service):Controller
{
    [HttpGet]public IActionResult Index(){var u=HttpContext.Session.GetUsuario();if(u is null)return RedirectToAction("Login","Clientes");return View("~/Views/Clientes/Perfil.cshtml",service.Obter(u.ClienteId));}
    [HttpPost,ValidateAntiForgeryToken]public IActionResult Index(Cliente model){var u=HttpContext.Session.GetUsuario();if(u is null)return RedirectToAction("Login","Clientes");model.ClienteId=u.ClienteId;if(!ModelState.IsValid)return View("~/Views/Clientes/Perfil.cshtml",model);service.Atualizar(model);HttpContext.Session.SetUsuario(u with{Nome=model.Nome,Email=model.Email});TempData["Sucesso"]="Perfil atualizado.";return RedirectToAction(nameof(Index));}
}
