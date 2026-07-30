using Microsoft.AspNetCore.Mvc;
using RocketPizza.Extensions;
using RocketPizza.Models;
using RocketPizza.Services;
namespace RocketPizza.Controllers;
public sealed class ContatoController(EmailService service):Controller
{
    [HttpGet]public IActionResult Index()=>View(new Contato());
    [HttpPost,ValidateAntiForgeryToken]public IActionResult Index(Contato model){if(!ModelState.IsValid)return View(model);model.ClienteId=HttpContext.Session.GetUsuario()?.ClienteId;service.EnviarContato(model);TempData["Sucesso"]="Mensagem enviada com sucesso.";return RedirectToAction(nameof(Index));}
}
