using Microsoft.Data.SqlClient;

namespace RocketPizza.Data;

public sealed class AppDbContext(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("RocketPizza")
        ?? throw new InvalidOperationException("Connection string RocketPizza não configurada.");

    public SqlConnection CreateConnection() => new(_connectionString);
}
