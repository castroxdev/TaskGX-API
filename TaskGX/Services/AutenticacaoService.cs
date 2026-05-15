using System.Security.Cryptography;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskGX.API.Models;
using TaskGX.API.Repositories;

namespace TaskGX.API.Services
{
    public class AutenticacaoService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly ConfiguracoesGoogleAuth _configuracoesGoogleAuth;

        public AutenticacaoService(
            UsuarioRepository usuarioRepository,
            IOptions<ConfiguracoesGoogleAuth> configuracoesGoogleAuth)
        {
            _usuarioRepository = usuarioRepository;
            _configuracoesGoogleAuth = configuracoesGoogleAuth.Value;
        }

        public async Task<Usuario?> AutenticarAsync(string email, string senhaInformada)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return null;

            if (!SenhaService.Verificar(senhaInformada, usuario.SenhaHash))
                return null;

            if (!usuario.Ativo || !usuario.EmailVerificado)
                return null;

            return usuario;
        }

        public async Task<(bool Sucesso, string Mensagem, int StatusCode, Usuario? Usuario)> AutenticarComGoogleAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
                return (false, "Token Google nao informado.", StatusCodes.Status400BadRequest, null);

            var clientId = (_configuracoesGoogleAuth.ClientId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(clientId))
                return (false, "Google Client ID nao configurado.", StatusCodes.Status500InternalServerError, null);

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken.Trim(),
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = [clientId]
                    });
            }
            catch (InvalidJwtException)
            {
                return (false, "Token Google invalido.", StatusCodes.Status401Unauthorized, null);
            }
            catch
            {
                return (false, "Nao foi possivel validar o token Google no momento.", StatusCodes.Status503ServiceUnavailable, null);
            }

            if (string.IsNullOrWhiteSpace(payload.Email))
                return (false, "Token Google sem email.", StatusCodes.Status401Unauthorized, null);

            if (!payload.EmailVerified)
                return (false, "Email Google nao verificado.", StatusCodes.Status401Unauthorized, null);

            var email = NormalizarEmail(payload.Email);
            var usuario = await _usuarioRepository.ObterPorEmailIgnorandoMaiusculasAsync(email);
            if (usuario != null)
            {
                if (!usuario.Ativo)
                    return (false, "Usuario inativo.", StatusCodes.Status401Unauthorized, null);

                return (true, "Login com Google realizado com sucesso.", StatusCodes.Status200OK, usuario);
            }

            var novoUsuario = new Usuario
            {
                Nome = NormalizarTextoLimite(payload.Name, email, 100) ?? email,
                Email = email,
                SenhaHash = SenhaService.GerarHash(GerarSenhaAleatoriaLoginExterno()),
                Avatar = NormalizarAvatarGoogle(payload.Picture),
                Ativo = true,
                EmailVerificado = true,
                CodigoVerificacao = null,
                CodigoVerificacaoExpiracao = null,
                CriadoEm = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            try
            {
                await _usuarioRepository.InserirAsync(novoUsuario);
            }
            catch (DbUpdateException)
            {
                var usuarioCriadoEmParalelo = await _usuarioRepository.ObterPorEmailIgnorandoMaiusculasAsync(email);
                if (usuarioCriadoEmParalelo == null)
                    throw;

                if (!usuarioCriadoEmParalelo.Ativo)
                    return (false, "Usuario inativo.", StatusCodes.Status401Unauthorized, null);

                return (true, "Login com Google realizado com sucesso.", StatusCodes.Status200OK, usuarioCriadoEmParalelo);
            }

            return (true, "Login com Google realizado com sucesso.", StatusCodes.Status200OK, novoUsuario);
        }

        private static string GerarSenhaAleatoriaLoginExterno()
        {
            return $"{Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))}Aa1!";
        }

        private static string NormalizarEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static string? NormalizarAvatarGoogle(string? picture)
        {
            var avatar = picture?.Trim();
            if (string.IsNullOrWhiteSpace(avatar))
                return null;

            return avatar.Length <= 255 ? avatar : null;
        }

        private static string? NormalizarTextoLimite(string? valor, string? valorPadrao, int tamanhoMaximo)
        {
            var texto = string.IsNullOrWhiteSpace(valor) ? valorPadrao : valor.Trim();
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            return texto.Length <= tamanhoMaximo ? texto : texto[..tamanhoMaximo];
        }
    }
}
