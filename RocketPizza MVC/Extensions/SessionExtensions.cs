using System.Text.Json;
using RocketPizza.Models;

namespace RocketPizza.Extensions;

public static class SessionExtensions
{
    private const string Key = "usuario";
    public static void SetUsuario(this ISession session, UsuarioSessao usuario) =>
        session.SetString(Key, JsonSerializer.Serialize(usuario));
    public static UsuarioSessao? GetUsuario(this ISession session)
    {
        var json = session.GetString(Key);
        return json is null ? null : JsonSerializer.Deserialize<UsuarioSessao>(json);
    }
}
