using Microsoft.Data.SqlClient;
using RocketPizza.Data;
using RocketPizza.Models;

namespace RocketPizza.Repositories;
public sealed class ClienteRepository(AppDbContext context):IClienteRepository
{
    public UsuarioSessao? Autenticar(string login,string senha){using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("dbo.sp_Autenticar",cn){CommandType=System.Data.CommandType.StoredProcedure};cmd.Parameters.AddWithValue("@Login",login);cmd.Parameters.AddWithValue("@Senha",senha);using var r=cmd.ExecuteReader();return r.Read()?new(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3)):null;}
    public int Cadastrar(CadastroViewModel m){if(m.DataNascimento>DateTime.Today.AddYears(-13))throw new InvalidOperationException("É necessário ter pelo menos 13 anos.");using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("dbo.sp_ClienteInserir",cn){CommandType=System.Data.CommandType.StoredProcedure};cmd.Parameters.AddWithValue("@Nome",m.Nome);cmd.Parameters.AddWithValue("@Email",m.Email);cmd.Parameters.AddWithValue("@Telefone",m.Telefone);cmd.Parameters.AddWithValue("@DataNascimento",m.DataNascimento);cmd.Parameters.AddWithValue("@Senha",m.Senha);return Convert.ToInt32(cmd.ExecuteScalar());}
    public Cliente? Obter(int id)=>ListarInterno(id).FirstOrDefault();
    public IReadOnlyList<Cliente> Listar()=>ListarInterno(null);
    private List<Cliente> ListarInterno(int? id){using var cn=context.CreateConnection();cn.Open();var sql="SELECT ClienteId,Nome,Email,Telefone,DataNascimento,Ativo,CriadoEm FROM dbo.Clientes WHERE Email<>'admin@rocketpizza.local'"+(id.HasValue?" AND ClienteId=@Id":"")+" ORDER BY ClienteId DESC";using var cmd=new SqlCommand(sql,cn);if(id.HasValue)cmd.Parameters.AddWithValue("@Id",id);using var r=cmd.ExecuteReader();var l=new List<Cliente>();while(r.Read())l.Add(new(){ClienteId=r.GetInt32(0),Nome=r.GetString(1),Email=r.GetString(2),Telefone=r.GetString(3),DataNascimento=r.IsDBNull(4)?null:r.GetDateTime(4),Ativo=r.GetBoolean(5),CriadoEm=r.GetDateTime(6)});return l;}
    public void Atualizar(Cliente c){using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Clientes SET Nome=@Nome,Email=@Email,Telefone=@Telefone,AtualizadoEm=SYSDATETIME() WHERE ClienteId=@Id",cn);cmd.Parameters.AddWithValue("@Nome",c.Nome);cmd.Parameters.AddWithValue("@Email",c.Email);cmd.Parameters.AddWithValue("@Telefone",c.Telefone);cmd.Parameters.AddWithValue("@Id",c.ClienteId);cmd.ExecuteNonQuery();}
    public void AlternarAtivo(int id){using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("UPDATE dbo.Clientes SET Ativo=IIF(Ativo=1,0,1),AtualizadoEm=SYSDATETIME() WHERE ClienteId=@Id",cn);cmd.Parameters.AddWithValue("@Id",id);cmd.ExecuteNonQuery();}
    public void Excluir(int id){using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("dbo.sp_ClienteExcluir",cn){CommandType=System.Data.CommandType.StoredProcedure};cmd.Parameters.AddWithValue("@ClienteId",id);cmd.ExecuteNonQuery();}
}
