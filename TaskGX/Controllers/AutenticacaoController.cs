using Microsoft.AspNetCore.Mvc;
using TaskGX.API.DTOs;
using TaskGX.API.Services;

namespace TaskGX.API.Controllers
{
    [ApiController]
    [Route("api/autenticacao")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly AutenticacaoService _autenticacaoService;
        private readonly TokenService _tokenService;

        public AutenticacaoController(AutenticacaoService autenticacaoService, TokenService tokenService)
        {
            _autenticacaoService = autenticacaoService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Autenticar([FromBody] LoginDTO dtoLogin)
        {
            var usuario = await _autenticacaoService.AutenticarAsync(dtoLogin.Email, dtoLogin.Senha);
            if (usuario == null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Falha na autenticacao.",
                    Detail = "Credenciais invalidas ou usuario nao autorizado.",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            var token = _tokenService.CriarToken(usuario);

            return Ok(new
            {
                token,
                usuario = new { usuario.ID, usuario.Nome, usuario.Email }
            });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> AutenticarComGoogle([FromBody] GoogleLoginDTO? dtoLogin)
        {
            if (dtoLogin == null || string.IsNullOrWhiteSpace(dtoLogin.IdToken))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Falha na autenticacao com Google.",
                    Detail = "Token Google nao informado.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var resultado = await _autenticacaoService.AutenticarComGoogleAsync(dtoLogin.IdToken);
            if (!resultado.Sucesso || resultado.Usuario == null)
            {
                var problema = new ProblemDetails
                {
                    Title = "Falha na autenticacao com Google.",
                    Detail = resultado.Mensagem,
                    Status = resultado.StatusCode
                };

                if (resultado.StatusCode == StatusCodes.Status401Unauthorized)
                    return Unauthorized(problema);

                return StatusCode(resultado.StatusCode, problema);
            }

            var token = _tokenService.CriarToken(resultado.Usuario);

            return Ok(new
            {
                token,
                usuario = new
                {
                    resultado.Usuario.ID,
                    resultado.Usuario.Nome,
                    resultado.Usuario.Email,
                    resultado.Usuario.Avatar
                }
            });
        }
    }
}
