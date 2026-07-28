using System.ComponentModel.DataAnnotations;
namespace RocketPizza.Models;
public sealed class Contato
{
    public int ContatoId { get; set; }
    public int? ClienteId { get; set; }
    [Required, StringLength(120)] public string Nome { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [StringLength(15)] public string? Telefone { get; set; }
    [Required, StringLength(80)] public string Assunto { get; set; } = "";
    [Required, StringLength(500, MinimumLength = 10)] public string Mensagem { get; set; } = "";
    public DateTime CriadoEm { get; set; }
}
