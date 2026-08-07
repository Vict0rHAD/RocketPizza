using Microsoft.Data.SqlClient;
using System.Data;

namespace RocketPizza.Dados;

public sealed class RocketPizzaRepositorio
{
    private readonly string _conexao;

    public RocketPizzaRepositorio(string? conexao = null)
    {
        _conexao = string.IsNullOrWhiteSpace(conexao) ? SqlScripts.ConexaoPadrao : conexao;
    }

    public async Task CriarBancoAsync()
    {
        try
        {
            await using (var master = new SqlConnection(SqlScripts.ConexaoMaster))
            {
                await master.OpenAsync();
                await using var cmd = new SqlCommand(SqlScripts.CriarBanco, master);
                await cmd.ExecuteNonQueryAsync();
            }

            await using var conn = await AbrirAsync();
            await using var schema = new SqlCommand(SqlScripts.CriarEstrutura, conn);
            await schema.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Nao foi possivel criar ou acessar o banco SQL Server. Execute os scripts da pasta database ou ajuste a connection string. Detalhe: " + ex.Message, ex);
        }
    }

    public async Task<LoginResultado> LoginAsync(string email, string senha)
    {
        const string sql = "SELECT UsuarioId, Nome, Email, Telefone, Senha, Perfil FROM dbo.Usuarios WHERE Email=@Email AND Senha=@Senha";
        await using var conn = await AbrirAsync();
        await using var cmd = NovoComando(sql, conn, ("@Email", email.Trim().ToLowerInvariant()), ("@Senha", senha));
        await using var rd = await cmd.ExecuteReaderAsync();

        if (!await rd.ReadAsync())
        {
            await InserirLogAsync(email, "Login negado", "E-mail ou senha invalidos.");
            return new LoginResultado { Sucesso = false, Mensagem = "E-mail ou senha invalidos." };
        }

        var usuario = LerUsuario(rd);
        await InserirLogAsync(usuario.Email, "Login", $"Usuario {usuario.Perfil} entrou no sistema.");
        return new LoginResultado { Sucesso = true, Mensagem = "Login realizado com sucesso.", Usuario = usuario };
    }

    public Task<IReadOnlyList<Usuario>> ListarUsuariosAsync() =>
        ListarAsync("SELECT UsuarioId, Nome, Email, Telefone, Senha, Perfil FROM dbo.Usuarios ORDER BY Nome", LerUsuario);

    public Task<Usuario?> ObterUsuarioAsync(int id) =>
        ObterAsync("SELECT UsuarioId, Nome, Email, Telefone, Senha, Perfil FROM dbo.Usuarios WHERE UsuarioId=@Id", LerUsuario, ("@Id", id));

    public async Task SalvarUsuarioAsync(Usuario usuario, string autor)
    {
        ExigirUsuarioValido(usuario);
        var sql = usuario.UsuarioId == 0
            ? "INSERT INTO dbo.Usuarios (Nome, Email, Telefone, Senha, Perfil) VALUES (@Nome, @Email, @Telefone, @Senha, @Perfil)"
            : "UPDATE dbo.Usuarios SET Nome=@Nome, Email=@Email, Telefone=@Telefone, Senha=@Senha, Perfil=@Perfil WHERE UsuarioId=@Id";
        await ExecutarAsync(sql, ParametrosUsuario(usuario));
        await InserirLogAsync(autor, usuario.UsuarioId == 0 ? "Usuario criado" : "Usuario atualizado", usuario.Email);
    }

    public async Task ExcluirUsuarioAsync(int id, string autor)
    {
        await ExecutarAsync("DELETE FROM dbo.Usuarios WHERE UsuarioId=@Id", ("@Id", id));
        await InserirLogAsync(autor, "Usuario excluido", $"UsuarioId {id}");
    }

    public Task<IReadOnlyList<Cliente>> ListarClientesAsync() =>
        ListarAsync("SELECT ClienteId, UsuarioId, Nome, Email, Telefone, Endereco, Numero FROM dbo.Clientes ORDER BY Nome", LerCliente);

    public Task<Cliente?> ObterClienteAsync(int id) =>
        ObterAsync("SELECT ClienteId, UsuarioId, Nome, Email, Telefone, Endereco, Numero FROM dbo.Clientes WHERE ClienteId=@Id", LerCliente, ("@Id", id));

    public Task<Cliente?> ObterClientePorEmailAsync(string email) =>
        ObterAsync("SELECT ClienteId, UsuarioId, Nome, Email, Telefone, Endereco, Numero FROM dbo.Clientes WHERE Email=@Email", LerCliente, ("@Email", email.Trim().ToLowerInvariant()));

    public async Task<Usuario> CadastrarClienteAsync(Cliente cliente, string senha)
    {
        ExigirClienteValido(cliente);
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6) throw new ArgumentException("A senha deve ter pelo menos 6 caracteres.");

        var email = cliente.Email.Trim().ToLowerInvariant();
        await using var conn = await AbrirAsync();

        await using (var existe = NovoComando("SELECT COUNT(1) FROM dbo.Usuarios WHERE Email=@Email", conn, ("@Email", email)))
        {
            if (Convert.ToInt32(await existe.ExecuteScalarAsync()) > 0)
            {
                throw new InvalidOperationException("Ja existe um usuario cadastrado com este e-mail.");
            }
        }

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await using var usuarioCmd = NovoComando("""
INSERT INTO dbo.Usuarios (Nome, Email, Telefone, Senha, Perfil)
OUTPUT INSERTED.UsuarioId
VALUES (@Nome, @Email, @Telefone, @Senha, N'Cliente')
""", conn, ("@Nome", cliente.Nome.Trim()), ("@Email", email), ("@Telefone", cliente.Telefone.Trim()), ("@Senha", senha));
            usuarioCmd.Transaction = (SqlTransaction)tx;
            var usuarioId = Convert.ToInt32(await usuarioCmd.ExecuteScalarAsync());

            await using var clienteCmd = NovoComando("""
INSERT INTO dbo.Clientes (UsuarioId, Nome, Email, Telefone, Endereco, Numero)
VALUES (@UsuarioId, @Nome, @Email, @Telefone, @Endereco, @Numero)
""", conn,
                ("@UsuarioId", usuarioId),
                ("@Nome", cliente.Nome.Trim()),
                ("@Email", email),
                ("@Telefone", cliente.Telefone.Trim()),
                ("@Endereco", cliente.Endereco.Trim()),
                ("@Numero", cliente.Numero.Trim()));
            clienteCmd.Transaction = (SqlTransaction)tx;
            await clienteCmd.ExecuteNonQueryAsync();

            await using var logCmd = NovoComando("INSERT INTO dbo.LogsSistema (UsuarioEmail, Acao, Detalhes) VALUES (@UsuarioEmail, @Acao, @Detalhes)",
                conn, ("@UsuarioEmail", email), ("@Acao", "Cadastro"), ("@Detalhes", "Cliente criou acesso proprio."));
            logCmd.Transaction = (SqlTransaction)tx;
            await logCmd.ExecuteNonQueryAsync();

            await tx.CommitAsync();
            return new Usuario { UsuarioId = usuarioId, Nome = cliente.Nome.Trim(), Email = email, Telefone = cliente.Telefone.Trim(), Senha = senha, Perfil = "Cliente" };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task SalvarClienteAsync(Cliente cliente, string autor)
    {
        ExigirClienteValido(cliente);
        var sql = cliente.ClienteId == 0
            ? "INSERT INTO dbo.Clientes (UsuarioId, Nome, Email, Telefone, Endereco, Numero) VALUES (@UsuarioId, @Nome, @Email, @Telefone, @Endereco, @Numero)"
            : "UPDATE dbo.Clientes SET UsuarioId=@UsuarioId, Nome=@Nome, Email=@Email, Telefone=@Telefone, Endereco=@Endereco, Numero=@Numero WHERE ClienteId=@Id";
        await ExecutarAsync(sql, ParametrosCliente(cliente));
        await InserirLogAsync(autor, cliente.ClienteId == 0 ? "Cliente criado" : "Cliente atualizado", cliente.Email);
    }

    public async Task ExcluirClienteAsync(int id, string autor)
    {
        await ExecutarAsync("DELETE FROM dbo.Clientes WHERE ClienteId=@Id", ("@Id", id));
        await InserirLogAsync(autor, "Cliente excluido", $"ClienteId {id}");
    }

    public Task<IReadOnlyList<Produto>> ListarProdutosAsync(bool somenteAtivos = false)
    {
        var sql = "SELECT ProdutoId, Nome, Categoria, Preco, Ativo FROM dbo.Produtos" + (somenteAtivos ? " WHERE Ativo=1" : "") + " ORDER BY Categoria, Nome";
        return ListarAsync(sql, LerProduto);
    }

    public Task<Produto?> ObterProdutoAsync(int id) =>
        ObterAsync("SELECT ProdutoId, Nome, Categoria, Preco, Ativo FROM dbo.Produtos WHERE ProdutoId=@Id", LerProduto, ("@Id", id));

    public async Task SalvarProdutoAsync(Produto produto, string autor)
    {
        ExigirProdutoValido(produto);
        var sql = produto.ProdutoId == 0
            ? "INSERT INTO dbo.Produtos (Nome, Categoria, Preco, Ativo) VALUES (@Nome, @Categoria, @Preco, @Ativo)"
            : "UPDATE dbo.Produtos SET Nome=@Nome, Categoria=@Categoria, Preco=@Preco, Ativo=@Ativo WHERE ProdutoId=@Id";
        await ExecutarAsync(sql, ("@Id", produto.ProdutoId), ("@Nome", produto.Nome), ("@Categoria", produto.Categoria), ("@Preco", produto.Preco), ("@Ativo", produto.Ativo));
        await InserirLogAsync(autor, produto.ProdutoId == 0 ? "Produto criado" : "Produto atualizado", produto.Nome);
    }

    public async Task ExcluirProdutoAsync(int id, string autor)
    {
        await ExecutarAsync("DELETE FROM dbo.Produtos WHERE ProdutoId=@Id", ("@Id", id));
        await InserirLogAsync(autor, "Produto excluido", $"ProdutoId {id}");
    }

    public Task<IReadOnlyList<Pedido>> ListarPedidosAsync(string? emailCliente = null)
    {
        var sql = """
SELECT p.PedidoId, p.ClienteId, c.Nome AS ClienteNome, p.CriadoEm, p.Status, p.Total, p.Observacao
FROM dbo.Pedidos p
INNER JOIN dbo.Clientes c ON c.ClienteId = p.ClienteId
""";
        var parametros = new List<(string, object?)>();
        if (!string.IsNullOrWhiteSpace(emailCliente))
        {
            sql += " WHERE c.Email=@Email";
            parametros.Add(("@Email", emailCliente.Trim().ToLowerInvariant()));
        }

        sql += " ORDER BY p.CriadoEm DESC";
        return ListarAsync(sql, LerPedido, parametros.ToArray());
    }

    public Task<Pedido?> ObterPedidoAsync(int id) =>
        ObterAsync("""
SELECT p.PedidoId, p.ClienteId, c.Nome AS ClienteNome, p.CriadoEm, p.Status, p.Total, p.Observacao
FROM dbo.Pedidos p
INNER JOIN dbo.Clientes c ON c.ClienteId = p.ClienteId
WHERE p.PedidoId=@Id
""", LerPedido, ("@Id", id));

    public async Task<int> CriarPedidoAsync(int clienteId, int produtoId, int quantidade, string observacao, string autor)
    {
        if (quantidade <= 0) throw new ArgumentException("Quantidade precisa ser maior que zero.");

        await using var conn = await AbrirAsync();
        await using var tx = await conn.BeginTransactionAsync();
        var preco = Convert.ToDecimal(await ScalarAsync(conn, (SqlTransaction)tx, "SELECT Preco FROM dbo.Produtos WHERE ProdutoId=@ProdutoId AND Ativo=1", ("@ProdutoId", produtoId)));
        var total = preco * quantidade;

        await using var pedido = NovoComando("INSERT INTO dbo.Pedidos (ClienteId, Status, Total, Observacao) OUTPUT INSERTED.PedidoId VALUES (@ClienteId, N'Recebido', @Total, @Observacao)", conn, ("@ClienteId", clienteId), ("@Total", total), ("@Observacao", observacao));
        pedido.Transaction = (SqlTransaction)tx;
        var pedidoId = Convert.ToInt32(await pedido.ExecuteScalarAsync());

        await using var item = NovoComando("INSERT INTO dbo.PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario) VALUES (@PedidoId, @ProdutoId, @Quantidade, @Preco)", conn, ("@PedidoId", pedidoId), ("@ProdutoId", produtoId), ("@Quantidade", quantidade), ("@Preco", preco));
        item.Transaction = (SqlTransaction)tx;
        await item.ExecuteNonQueryAsync();
        await tx.CommitAsync();

        await InserirLogAsync(autor, "Pedido criado", $"PedidoId {pedidoId}");
        return pedidoId;
    }

    public async Task AtualizarPedidoAsync(Pedido pedido, string autor)
    {
        await ExecutarAsync("UPDATE dbo.Pedidos SET ClienteId=@ClienteId, Status=@Status, Total=@Total, Observacao=@Observacao WHERE PedidoId=@Id",
            ("@Id", pedido.PedidoId), ("@ClienteId", pedido.ClienteId), ("@Status", pedido.Status), ("@Total", pedido.Total), ("@Observacao", pedido.Observacao));
        await InserirLogAsync(autor, "Pedido atualizado", $"PedidoId {pedido.PedidoId}");
    }

    public async Task ExcluirPedidoAsync(int id, string autor)
    {
        await ExecutarAsync("DELETE FROM dbo.Pedidos WHERE PedidoId=@Id", ("@Id", id));
        await InserirLogAsync(autor, "Pedido excluido", $"PedidoId {id}");
    }

    public Task<IReadOnlyList<PedidoItem>> ListarItensPedidoAsync(int pedidoId) =>
        ListarAsync("""
SELECT i.PedidoItemId, i.PedidoId, i.ProdutoId, pr.Nome AS ProdutoNome, i.Quantidade, i.PrecoUnitario
FROM dbo.PedidoItens i
INNER JOIN dbo.Produtos pr ON pr.ProdutoId = i.ProdutoId
WHERE i.PedidoId=@PedidoId
ORDER BY i.PedidoItemId
""", LerPedidoItem, ("@PedidoId", pedidoId));

    public Task<IReadOnlyList<LogSistema>> ListarLogsAsync() =>
        ListarAsync("SELECT TOP 100 LogSistemaId, CriadoEm, UsuarioEmail, Acao, Detalhes FROM dbo.LogsSistema ORDER BY CriadoEm DESC", LerLog);

    public Task<DataTable> TabelaAsync(string sql) => ConsultarTabelaAsync(sql);

    public async Task<DataTable> ConsultarTabelaAsync(string sql)
    {
        await using var conn = await AbrirAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        var tabela = new DataTable();
        tabela.Load(reader);
        return tabela;
    }

    public async Task InserirLogAsync(string usuarioEmail, string acao, string detalhes)
    {
        await ExecutarAsync("INSERT INTO dbo.LogsSistema (UsuarioEmail, Acao, Detalhes) VALUES (@UsuarioEmail, @Acao, @Detalhes)",
            ("@UsuarioEmail", usuarioEmail), ("@Acao", acao), ("@Detalhes", detalhes));
    }

    private async Task<SqlConnection> AbrirAsync()
    {
        try
        {
            var conn = new SqlConnection(_conexao);
            await conn.OpenAsync();
            return conn;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Falha ao abrir conexao com o SQL Server configurado. Verifique se o banco foi criado e se a connection string esta correta. Detalhe: " + ex.Message, ex);
        }
    }

    private static SqlCommand NovoComando(string sql, SqlConnection conn, params (string Nome, object? Valor)[] parametros)
    {
        var cmd = new SqlCommand(sql, conn);
        foreach (var (nome, valor) in parametros)
        {
            cmd.Parameters.AddWithValue(nome, valor ?? DBNull.Value);
        }

        return cmd;
    }

    private async Task ExecutarAsync(string sql, params (string Nome, object? Valor)[] parametros)
    {
        await using var conn = await AbrirAsync();
        await using var cmd = NovoComando(sql, conn, parametros);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<object?> ScalarAsync(SqlConnection conn, SqlTransaction tx, string sql, params (string Nome, object? Valor)[] parametros)
    {
        await using var cmd = NovoComando(sql, conn, parametros);
        cmd.Transaction = tx;
        return await cmd.ExecuteScalarAsync();
    }

    private async Task<IReadOnlyList<T>> ListarAsync<T>(string sql, Func<SqlDataReader, T> mapear, params (string Nome, object? Valor)[] parametros)
    {
        var lista = new List<T>();
        await using var conn = await AbrirAsync();
        await using var cmd = NovoComando(sql, conn, parametros);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) lista.Add(mapear(rd));
        return lista;
    }

    private async Task<T?> ObterAsync<T>(string sql, Func<SqlDataReader, T> mapear, params (string Nome, object? Valor)[] parametros)
    {
        await using var conn = await AbrirAsync();
        await using var cmd = NovoComando(sql, conn, parametros);
        await using var rd = await cmd.ExecuteReaderAsync();
        return await rd.ReadAsync() ? mapear(rd) : default;
    }

    private static Usuario LerUsuario(SqlDataReader rd) => new()
    {
        UsuarioId = rd.GetInt32("UsuarioId"),
        Nome = rd.GetString("Nome"),
        Email = rd.GetString("Email"),
        Telefone = rd.GetString("Telefone"),
        Senha = rd.GetString("Senha"),
        Perfil = rd.GetString("Perfil")
    };

    private static Cliente LerCliente(SqlDataReader rd) => new()
    {
        ClienteId = rd.GetInt32("ClienteId"),
        UsuarioId = rd.IsDBNull("UsuarioId") ? null : rd.GetInt32("UsuarioId"),
        Nome = rd.GetString("Nome"),
        Email = rd.GetString("Email"),
        Telefone = rd.GetString("Telefone"),
        Endereco = rd.GetString("Endereco"),
        Numero = rd.GetString("Numero")
    };

    private static Produto LerProduto(SqlDataReader rd) => new()
    {
        ProdutoId = rd.GetInt32("ProdutoId"),
        Nome = rd.GetString("Nome"),
        Categoria = rd.GetString("Categoria"),
        Preco = rd.GetDecimal("Preco"),
        Ativo = rd.GetBoolean("Ativo")
    };

    private static Pedido LerPedido(SqlDataReader rd) => new()
    {
        PedidoId = rd.GetInt32("PedidoId"),
        ClienteId = rd.GetInt32("ClienteId"),
        ClienteNome = rd.GetString("ClienteNome"),
        CriadoEm = rd.GetDateTime("CriadoEm"),
        Status = rd.GetString("Status"),
        Total = rd.GetDecimal("Total"),
        Observacao = rd.GetString("Observacao")
    };

    private static PedidoItem LerPedidoItem(SqlDataReader rd) => new()
    {
        PedidoItemId = rd.GetInt32("PedidoItemId"),
        PedidoId = rd.GetInt32("PedidoId"),
        ProdutoId = rd.GetInt32("ProdutoId"),
        ProdutoNome = rd.GetString("ProdutoNome"),
        Quantidade = rd.GetInt32("Quantidade"),
        PrecoUnitario = rd.GetDecimal("PrecoUnitario")
    };

    private static LogSistema LerLog(SqlDataReader rd) => new()
    {
        LogSistemaId = rd.GetInt32("LogSistemaId"),
        CriadoEm = rd.GetDateTime("CriadoEm"),
        UsuarioEmail = rd.GetString("UsuarioEmail"),
        Acao = rd.GetString("Acao"),
        Detalhes = rd.GetString("Detalhes")
    };

    private static (string, object?)[] ParametrosUsuario(Usuario usuario) =>
    [
        ("@Id", usuario.UsuarioId), ("@Nome", usuario.Nome.Trim()), ("@Email", usuario.Email.Trim().ToLowerInvariant()),
        ("@Telefone", usuario.Telefone.Trim()), ("@Senha", usuario.Senha), ("@Perfil", usuario.Perfil)
    ];

    private static (string, object?)[] ParametrosCliente(Cliente cliente) =>
    [
        ("@Id", cliente.ClienteId), ("@UsuarioId", cliente.UsuarioId), ("@Nome", cliente.Nome.Trim()),
        ("@Email", cliente.Email.Trim().ToLowerInvariant()), ("@Telefone", cliente.Telefone.Trim()),
        ("@Endereco", cliente.Endereco.Trim()), ("@Numero", cliente.Numero.Trim())
    ];

    private static void ExigirUsuarioValido(Usuario usuario)
    {
        if (!Validacoes.ApenasLetras(usuario.Nome)) throw new ArgumentException("Nome aceita apenas letras.");
        if (!Validacoes.TelefoneValido(usuario.Telefone)) throw new ArgumentException("Telefone deve seguir o padrao (11) 99999-9999.");
    }

    private static void ExigirClienteValido(Cliente cliente)
    {
        if (!Validacoes.ApenasLetras(cliente.Nome)) throw new ArgumentException("Nome aceita apenas letras.");
        if (!Validacoes.TelefoneValido(cliente.Telefone)) throw new ArgumentException("Telefone deve seguir o padrao (11) 99999-9999.");
        if (!Validacoes.ApenasNumeros(cliente.Numero)) throw new ArgumentException("Numero aceita apenas numeros.");
    }

    private static void ExigirProdutoValido(Produto produto)
    {
        if (!Validacoes.ApenasLetras(produto.Categoria)) throw new ArgumentException("Categoria aceita apenas letras.");
        if (produto.Preco <= 0) throw new ArgumentException("Preco precisa ser maior que zero.");
    }
}
