namespace RocketPizza.Services;
public sealed class PagamentoService
{
    private static readonly string[] Formas=["Pix","Cartao","Dinheiro"];
    public void Validar(string forma){if(!Formas.Contains(forma))throw new InvalidOperationException("Forma de pagamento inválida.");}
}
