using Microsoft.AspNetCore.Mvc;
namespace RocketPizza.Controllers;
public sealed class HomeController:Controller{public IActionResult Index()=>View();}
