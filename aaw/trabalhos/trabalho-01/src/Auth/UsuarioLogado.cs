using System.Security.Claims;
using SuporteApi.Models;

namespace SuporteApi.Auth;

// Lê do token quem está fazendo a requisição.
public static class UsuarioLogado
{
    public static string Id(this ClaimsPrincipal user) =>
        user.FindFirstValue("sub") ?? throw new InvalidOperationException("Token sem a claim sub.");

    public static bool EhAdmin(this ClaimsPrincipal user) => user.IsInRole(Perfis.Admin);
}
