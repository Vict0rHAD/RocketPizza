using RocketPizza.Models;
using RocketPizza.Repositories;
namespace RocketPizza.Services;
public sealed class ClienteService(IClienteRepository repository)
{
    public UsuarioSessao? Autenticar(string login,string senha)=>repository.Autenticar(login.Trim(),senha);
    public int Cadastrar(CadastroViewModel model)=>repository.Cadastrar(model);
    public Cliente? Obter(int id)=>repository.Obter(id);
    public IReadOnlyList<Cliente> Listar()=>repository.Listar();
    public void Atualizar(Cliente cliente)=>repository.Atualizar(cliente);
    public void AlternarAtivo(int id)=>repository.AlternarAtivo(id);
    public void Excluir(int id)=>repository.Excluir(id);
}
