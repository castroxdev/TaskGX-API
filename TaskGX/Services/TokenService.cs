using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskGX.API.Models;

namespace TaskGX.API.Services
{
    public class TokenService
    {
        private readonly ConfiguracoesJwt _configuracoes;

        public TokenService(IOptions<ConfiguracoesJwt> configuracoes)
        {
            _configuracoes = configuracoes.Value;
        }

        public string CriarToken(Usuario usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.ID.ToString()),
                new(ClaimTypes.NameIdentifier, usuario.ID.ToString()),
                new(JwtRegisteredClaimNames.Email, usuario.Email),
                new(ClaimTypes.Email, usuario.Email),
                new("name", usuario.Nome),
                new(ClaimTypes.Name, usuario.Nome),
            };

            var chaveAssinatura = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracoes.Chave));
            var credenciais = new SigningCredentials(chaveAssinatura, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuracoes.Emissor,
                audience: _configuracoes.Audiencia,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_configuracoes.MinutosExpiracao),
                signingCredentials: credenciais);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static int ObterUsuarioId(ClaimsPrincipal usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            if (!TentarObterUsuarioId(usuario, out var id))
                throw new InvalidOperationException("Token sem ID de usuario (NameIdentifier/sub).");

            return id;
        }

        public static bool TentarObterUsuarioId(ClaimsPrincipal? usuario, out int id)
        {
            id = 0;

            if (usuario?.Identity?.IsAuthenticated != true)
                return false;

            var idTexto =
                usuario.FindFirstValue(ClaimTypes.NameIdentifier) ??
                usuario.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return !string.IsNullOrWhiteSpace(idTexto) &&
                int.TryParse(idTexto, out id);
        }

        public static string? ObterEmail(ClaimsPrincipal usuario)
        {
            if (usuario == null)
                return null;

            return usuario.FindFirstValue(ClaimTypes.Email)
                ?? usuario.FindFirstValue(JwtRegisteredClaimNames.Email);
        }

        public static string? ObterNome(ClaimsPrincipal usuario)
        {
            if (usuario == null)
                return null;

            return usuario.FindFirstValue(ClaimTypes.Name)
                ?? usuario.FindFirst("name")?.Value;
        }
    }
}
