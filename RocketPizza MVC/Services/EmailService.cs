using Microsoft.Data.SqlClient;
using RocketPizza.Data;
using RocketPizza.Models;
namespace RocketPizza.Services;
public sealed class EmailService(AppDbContext context,ILogger<EmailService> logger)
{
    public void EnviarContato(Contato contato)
    {
        using var cn=context.CreateConnection();cn.Open();using var cmd=new SqlCommand("INSERT dbo.Contatos(ClienteId,Nome,Email,Telefone,Assunto,Mensagem) VALUES(@Cliente,@Nome,@Email,@Telefone,@Assunto,@Mensagem)",cn);
        cmd.Parameters.AddWithValue("@Cliente",(object?)contato.ClienteId??DBNull.Value);cmd.Parameters.AddWithValue("@Nome",contato.Nome);cmd.Parameters.AddWithValue("@Email",contato.Email);
        cmd.Parameters.AddWithValue("@Telefone",(object?)contato.Telefone??DBNull.Value);cmd.Parameters.AddWithValue("@Assunto",contato.Assunto);cmd.Parameters.AddWithValue("@Mensagem",contato.Mensagem);cmd.ExecuteNonQuery();
        logger.LogInformation("Contato {Assunto} registrado para atendimento.",contato.Assunto);
    }
}
