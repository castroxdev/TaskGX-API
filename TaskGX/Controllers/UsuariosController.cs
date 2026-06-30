using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskGX.API.DTOs;
using TaskGX.API.Services;
using TaskGX.Data;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private const string TipoProblemaValidacao = "https://tools.ietf.org/html/rfc9110#section-15.5.1";

        private readonly TaskGXContext _contexto;
        private readonly AlteracaoEmailService _alteracaoEmailService;
        private readonly UsuarioService _usuarioService;

        public UsuariosController(
            TaskGXContext contexto,
            AlteracaoEmailService alteracaoEmailService,
            UsuarioService usuarioService)
        {
            _contexto = contexto;
            _alteracaoEmailService = alteracaoEmailService;
            _usuarioService = usuarioService;
        }

        [HttpGet("eu")]
        public async Task<IActionResult> ObterPerfil()
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var usuario = await _contexto.Usuarios
                .Where(item => item.ID == usuarioId)
                .Select(item => new
                {
                    item.ID,
                    item.Nome,
                    item.Email,
                    item.Avatar,
                    item.Ativo,
                    item.EmailVerificado,
                    item.CriadoEm,
                    item.DataAtualizacao
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return Unauthorized();

            return Ok(usuario);
        }

        [HttpPut("eu")]
        public async Task<IActionResult> AtualizarPerfil([FromBody] AtualizarUsuarioRequest requisicao)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(item => item.ID == usuarioId);
            if (usuario == null)
                return Unauthorized();

            var nome = (requisicao.Nome ?? string.Empty).Trim();
            var avatar = string.IsNullOrWhiteSpace(requisicao.Avatar) ? null : requisicao.Avatar.Trim();
            var erros = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(nome))
            {
                erros["nome"] = ["O nome e obrigatorio."];
            }
            else
            {
                if (nome.Length < 2)
                    erros["nome"] = ["O nome deve ter pelo menos 2 caracteres."];
                else if (nome.Length > 100)
                    erros["nome"] = ["O nome deve ter no maximo 100 caracteres."];
            }

            if (!string.IsNullOrEmpty(avatar) && avatar.Length > 255)
                erros["avatar"] = ["O avatar deve ter no maximo 255 caracteres."];

            if (erros.Count > 0)
                return BadRequest(CriarProblemaValidacao(erros));

            usuario.Nome = nome;
            usuario.Avatar = avatar;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("eu")]
        public async Task<IActionResult> EliminarConta()
        {
            if (!TokenService.TentarObterUsuarioId(User, out var usuarioId))
                return Unauthorized();

            var resultado = await _usuarioService.EliminarContaAsync(usuarioId);
            if (resultado == ResultadoEliminacaoConta.UsuarioNaoEncontrado)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("eu/senha")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest requisicao)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(item => item.ID == usuarioId);
            if (usuario == null)
                return Unauthorized();

            if (!SenhaService.Verificar(requisicao.SenhaAtual, usuario.SenhaHash))
            {
                return BadRequest(CriarProblema(
                    "Nao foi possivel alterar a senha.",
                    "A senha atual informada esta incorreta.",
                    StatusCodes.Status400BadRequest));
            }

            if (SenhaService.Verificar(requisicao.NovaSenha, usuario.SenhaHash))
            {
                return BadRequest(CriarProblema(
                    "Nao foi possivel alterar a senha.",
                    "A nova senha deve ser diferente da senha atual.",
                    StatusCodes.Status400BadRequest));
            }

            if (!string.Equals(requisicao.NovaSenha, requisicao.ConfirmarNovaSenha, StringComparison.Ordinal))
            {
                return BadRequest(CriarProblema(
                    "Nao foi possivel alterar a senha.",
                    "A confirmacao da nova senha nao confere.",
                    StatusCodes.Status400BadRequest));
            }

            if (!SenhaService.EhValida(requisicao.NovaSenha))
            {
                return BadRequest(CriarProblema(
                    "Nao foi possivel alterar a senha.",
                    "A nova senha nao atende aos requisitos de seguranca.",
                    StatusCodes.Status400BadRequest));
            }

            usuario.SenhaHash = SenhaService.GerarHash(requisicao.NovaSenha);
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("eu/email/solicitar-alteracao")]
        public async Task<IActionResult> SolicitarAlteracaoEmail([FromBody] SolicitarAlteracaoEmailRequest requisicao)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);
            var resultado = await _alteracaoEmailService.SolicitarAlteracaoAsync(usuarioId, requisicao.NovoEmail);

            if (!resultado.Sucesso)
            {
                var problema = CriarProblema(
                    "Nao foi possivel solicitar a alteracao de email.",
                    resultado.Mensagem,
                    resultado.StatusCode);

                if (resultado.StatusCode == StatusCodes.Status401Unauthorized)
                    return Unauthorized(problema);

                return StatusCode(resultado.StatusCode, problema);
            }

            return Ok(new { mensagem = resultado.Mensagem });
        }

        [HttpPost("eu/email/confirmar-alteracao")]
        public async Task<IActionResult> ConfirmarAlteracaoEmail([FromBody] ConfirmarAlteracaoEmailRequest requisicao)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);
            var resultado = await _alteracaoEmailService.ConfirmarAlteracaoAsync(usuarioId, requisicao.Codigo);

            if (!resultado.Sucesso)
            {
                var problema = CriarProblema(
                    "Nao foi possivel confirmar a alteracao de email.",
                    resultado.Mensagem,
                    resultado.StatusCode);

                if (resultado.StatusCode == StatusCodes.Status401Unauthorized)
                    return Unauthorized(problema);

                return StatusCode(resultado.StatusCode, problema);
            }

            return Ok(new { mensagem = resultado.Mensagem });
        }

        private static ProblemDetails CriarProblema(string titulo, string detalhe, int codigoStatus)
        {
            return new ProblemDetails
            {
                Title = titulo,
                Detail = detalhe,
                Status = codigoStatus
            };
        }

        private static ValidationProblemDetails CriarProblemaValidacao(IDictionary<string, string[]> erros)
        {
            return new ValidationProblemDetails(erros)
            {
                Title = "A requisicao contem dados invalidos.",
                Status = StatusCodes.Status400BadRequest,
                Type = TipoProblemaValidacao
            };
        }
    }
}
