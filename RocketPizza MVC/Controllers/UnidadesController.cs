using Microsoft.AspNetCore.Mvc;
using RocketPizza.Models;
namespace RocketPizza.Controllers;
public sealed class UnidadesController:Controller
{
    public IActionResult Index()=>View(new List<Unidade>{new(){UnidadeId=1,Nome="Rocket Pizza Centro",Endereco="Rua das Estrelas, 100 — Centro",Horario="Todos os dias, 18h às 23h",Telefone="(11) 4000-1000"},new(){UnidadeId=2,Nome="Rocket Pizza Zona Sul",Endereco="Av. Galáxia, 1500 — Zona Sul",Horario="Todos os dias, 18h às 00h",Telefone="(11) 4000-2000"}});
}
