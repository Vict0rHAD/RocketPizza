using System.ComponentModel.DataAnnotations;
namespace RocketPizza.Models;
public sealed class Endereco
{
    public int EnderecoId { get; set; }
    public int ClienteId { get; set; }
    [Required] public string Logradouro { get; set; } = "";
    [Required] public string Numero { get; set; } = "";
    public string? Complemento { get; set; }
    [Required] public string Bairro { get; set; } = "";
    [Required] public string Cidade { get; set; } = "";
    [Required, StringLength(2)] public string UF { get; set; } = "";
    [Required, StringLength(8)] public string CEP { get; set; } = "";
    public bool Principal { get; set; }
}
