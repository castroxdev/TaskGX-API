using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskGX.API.DTOs;
using TaskGX.API.Services;

namespace TaskGX.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/recuperacao-senha")]
    public class RecuperacaoSenhaController : ControllerBase
    {
        private readonly RecuperacaoSenhaService _recuperacaoSenhaService;

        public RecuperacaoSenhaController(RecuperacaoSenhaService recuperacaoSenhaService)
        {
            _recuperacaoSenhaService = recuperacaoSenhaService;
        }

        [HttpPost("solicitar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Solicitar(
            [FromBody] SolicitarRecuperacaoSenhaRequest requisicao)
        {
            var resultado = await _recuperacaoSenhaService.SolicitarAsync(requisicao.Email);
            return Ok(new { mensagem = resultado.Mensagem });
        }

        [HttpPost("validar-codigo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidarCodigo(
            [FromBody] ValidarCodigoRecuperacaoSenhaRequest requisicao)
        {
            var resultado = await _recuperacaoSenhaService.ValidarCodigoAsync(
                requisicao.Email,
                requisicao.Codigo);

            return CriarResposta(resultado, "Nao foi possivel validar o codigo.");
        }

        [HttpPost("redefinir")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Redefinir([FromBody] RedefinirSenhaRequest requisicao)
        {
            var resultado = await _recuperacaoSenhaService.RedefinirAsync(
                requisicao.Email,
                requisicao.Codigo,
                requisicao.NovaSenha,
                requisicao.ConfirmarNovaSenha);

            return CriarResposta(resultado, "Nao foi possivel redefinir a senha.");
        }

        private IActionResult CriarResposta(
            (bool Sucesso, string Mensagem, int StatusCode) resultado,
            string tituloErro)
        {
            if (resultado.Sucesso)
                return Ok(new { mensagem = resultado.Mensagem });

            return StatusCode(resultado.StatusCode, new ProblemDetails
            {
                Title = tituloErro,
                Detail = resultado.Mensagem,
                Status = resultado.StatusCode
            });
        }
    }
}
