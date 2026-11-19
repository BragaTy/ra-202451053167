using Microsoft.AspNetCore.Identity;
using SuporteApi.Auth;
using SuporteApi.Dtos;
using SuporteApi.Erros;
using SuporteApi.Models;
using SuporteApi.Repositories;

namespace SuporteApi.Services;

public class AuthService(IUsuarioRepository usuarios, TokenService tokens)
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public async Task<UsuarioResponse> RegistrarAsync(RegistrarRequest dados)
    {
        var email = dados.Email.Trim().ToLowerInvariant();
        if (await usuarios.ObterPorEmailAsync(email) is not null)
            throw new ConflitoException($"Já existe um usuário com o e-mail {email}.");

        // Todo registro público nasce "usuario": não há como pedir "admin" (Mass Assignment).
        var usuario = new Usuario { Nome = dados.Nome.Trim(), Email = email, Perfil = Perfis.Usuario };
        usuario.SenhaHash = _hasher.HashPassword(usuario, dados.Senha);
        await usuarios.CriarAsync(usuario);
        return new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest dados)
    {
        var usuario = await usuarios.ObterPorEmailAsync(dados.Email.Trim().ToLowerInvariant());

        // Mesma mensagem para e-mail inexistente e senha errada: não revela quais e-mails existem.
        if (usuario is null ||
            _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, dados.Senha) == PasswordVerificationResult.Failed)
            throw new NaoAutorizadoException("E-mail ou senha inválidos.");

        var (token, expiraEm) = tokens.Gerar(usuario);
        return new LoginResponse(token, expiraEm, usuario.Nome, usuario.Perfil);
    }

    // Cria o admin inicial na subida da API, a partir de Admin__Email e Admin__Senha.
    public async Task GarantirAdminAsync(string? email, string? senha)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha)) return;
        email = email.Trim().ToLowerInvariant();
        if (await usuarios.ObterPorEmailAsync(email) is not null) return;

        var admin = new Usuario { Nome = "Administrador", Email = email, Perfil = Perfis.Admin };
        admin.SenhaHash = _hasher.HashPassword(admin, senha);
        await usuarios.CriarAsync(admin);
    }
}
