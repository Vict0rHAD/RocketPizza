using System.ComponentModel.DataAnnotations;

namespace RocketPizza.Models;

public sealed class Cliente
{
    public int ClienteId { get; set; }
    [Required, StringLength(120, MinimumLength = 3)] public string Nome { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, RegularExpression(@"^\d{10,15}$")] public string Telefone { get; set; } = "";
    public DateTime? DataNascimento { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
}

public sealed record UsuarioSessao(int ClienteId, string Nome, string Email, string Perfil);

public sealed class LoginViewModel
{
    [Required] public string Login { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Senha { get; set; } = "";
    public string? Retorno { get; set; }
}

public sealed class CadastroViewModel
{
    [Required, StringLength(120, MinimumLength = 3)] public string Nome { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, RegularExpression(@"^\d{10,15}$")] public string Telefone { get; set; } = "";
    [Required, DataType(DataType.Date)] public DateTime DataNascimento { get; set; } = DateTime.Today.AddYears(-18);
    [Required, DataType(DataType.Password), StringLength(72, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9])\S+$")]
    public string Senha { get; set; } = "";
}
