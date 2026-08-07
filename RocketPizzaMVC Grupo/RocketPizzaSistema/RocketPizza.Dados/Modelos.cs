using System.ComponentModel.DataAnnotations;

namespace RocketPizza.Dados;

public sealed class Usuario
{
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [RegularExpression(Validacoes.PadraoApenasLetras, ErrorMessage = "Use apenas letras no nome.")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Informe o telefone.")]
    [RegularExpression(Validacoes.PadraoTelefone, ErrorMessage = "Use o padrao (11) 99999-9999.")]
    public string Telefone { get; set; } = "";

    [Required(ErrorMessage = "Informe a senha.")]
    public string Senha { get; set; } = "";

    [Required]
    public string Perfil { get; set; } = "Cliente";
}

public sealed class Cliente
{
    public int ClienteId { get; set; }
    public int? UsuarioId { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [RegularExpression(Validacoes.PadraoApenasLetras, ErrorMessage = "Use apenas letras no nome.")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Informe o telefone.")]
    [RegularExpression(Validacoes.PadraoTelefone, ErrorMessage = "Use o padrao (11) 99999-9999.")]
    public string Telefone { get; set; } = "";

    [Required(ErrorMessage = "Informe o endereco.")]
    public string Endereco { get; set; } = "";

    [Required(ErrorMessage = "Informe o numero.")]
    [RegularExpression(Validacoes.PadraoApenasNumeros, ErrorMessage = "Use apenas numeros.")]
    public string Numero { get; set; } = "";
}

public sealed class Produto
{
    public int ProdutoId { get; set; }

    [Required(ErrorMessage = "Informe o produto.")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Informe a categoria.")]
    [RegularExpression(Validacoes.PadraoApenasLetras, ErrorMessage = "Use apenas letras na categoria.")]
    public string Categoria { get; set; } = "Pizza";

    [Range(0.01, 9999, ErrorMessage = "Informe um preco valido.")]
    public decimal Preco { get; set; }

    public bool Ativo { get; set; } = true;
}

public sealed class Pedido
{
    public int PedidoId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = "";
    public DateTime CriadoEm { get; set; }
    public string Status { get; set; } = "Recebido";
    public decimal Total { get; set; }
    public string Observacao { get; set; } = "";
}

public sealed class PedidoItem
{
    public int PedidoItemId { get; set; }
    public int PedidoId { get; set; }
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = "";
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}

public sealed class LogSistema
{
    public int LogSistemaId { get; set; }
    public DateTime CriadoEm { get; set; }
    public string UsuarioEmail { get; set; } = "";
    public string Acao { get; set; } = "";
    public string Detalhes { get; set; } = "";
}

public sealed class LoginResultado
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = "";
    public Usuario? Usuario { get; set; }
}

