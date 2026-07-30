using System.ComponentModel.DataAnnotations;

namespace RocketPizza.Models;

public sealed class Produto
{
    public int ProdutoId { get; set; }
    public int CategoriaId { get; set; }
    public string Categoria { get; set; } = "";
    [Required, StringLength(100)] public string Nome { get; set; } = "";
    [Required, StringLength(300)] public string Descricao { get; set; } = "";
    [Range(0.01, 9999)] public decimal Preco { get; set; }
    public string? Imagem { get; set; }
    public bool Ativo { get; set; } = true;
}

public sealed class ProdutoFormViewModel
{
    public int? ProdutoId { get; set; }
    [Range(1, int.MaxValue)] public int CategoriaId { get; set; }
    [Required, StringLength(100)] public string Nome { get; set; } = "";
    [Required, StringLength(300)] public string Descricao { get; set; } = "";
    [Range(.01, 9999)] public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
}
