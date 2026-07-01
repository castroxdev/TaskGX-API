using TaskGX.API.Repositories;

namespace TaskGX.API.Services
{
    public class VerificacaoService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly EnvioEmailService _envioEmailService;

        public VerificacaoService(UsuarioRepository usuarioRepository, EnvioEmailService envioEmailService)
        {
            _usuarioRepository = usuarioRepository;
            _envioEmailService = envioEmailService;
        }

        public async Task<(bool Sucesso, string Mensagem)> VerificarEmailAsync(string email, string codigo)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            codigo = (codigo ?? string.Empty).Trim();

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return (false, "Email nao encontrado.");

            if (usuario.EmailVerificado)
                return (true, "Email ja verificado.");

            if (usuario.CodigoVerificacao != codigo)
                return (false, "Codigo invalido.");

            if (usuario.CodigoVerificacaoExpiracao is null || usuario.CodigoVerificacaoExpiracao < DateTime.UtcNow)
                return (false, "Codigo expirado.");

            await _usuarioRepository.AtualizarVerificacaoEmailAsync(usuario.ID, true, true, null, null);
            return (true, "Email verificado com sucesso.");
        }

        public async Task<(bool Sucesso, string Mensagem, int StatusCode)> ReenviarCodigoAsync(string email)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return (false, "Email nao encontrado.", StatusCodes.Status404NotFound);

            if (usuario.EmailVerificado)
                return (false, "O email informado ja esta verificado.", StatusCodes.Status409Conflict);

            var codigo = CadastroService.GerarCodigoVerificacao();
            var expiracao = DateTime.UtcNow.AddHours(24);

            var codigoAtualizado = await _usuarioRepository.AtualizarCodigoVerificacaoAsync(
                usuario.ID,
                codigo,
                expiracao);

            if (!codigoAtualizado)
                return (false, "O email informado ja esta verificado.", StatusCodes.Status409Conflict);

            try
            {
                await _envioEmailService.EnviarCodigoVerificacaoAsync(email, codigo, expiresAt: expiracao);
            }
            catch
            {
                return (false, "Nao foi possivel reenviar o codigo no momento.", StatusCodes.Status400BadRequest);
            }

            return (true, "Novo codigo enviado com sucesso.", StatusCodes.Status200OK);
        }
    }
}
