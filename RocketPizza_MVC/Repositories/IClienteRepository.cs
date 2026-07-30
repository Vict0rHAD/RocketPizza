using RocketPizza.Models;
namespace RocketPizza.Repositories;
public interface IClienteRepository
{
    UsuarioSessao? Autenticar(string login,string senha);
    int Cadastrar(CadastroViewModel cadastro);
    Cliente? Obter(int id);
    IReadOnlyList<Cliente> Listar();
    void Atualizar(Cliente cliente);
    void AlternarAtivo(int id);
    void Excluir(int id);
}
