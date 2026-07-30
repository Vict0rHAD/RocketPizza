namespace RocketPizza.Models;
public sealed class Categoria
{
    public int CategoriaId { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
}
