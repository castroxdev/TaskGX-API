using TaskGX.API.Repositories;

namespace TaskGX.API.Services
{
    public class AlteracaoEmailService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly EnvioEmailService _envioEmailService;

        public AlteracaoEmailService(UsuarioRepository usuarioRepository, EnvioEmailService envioEmailService)
        {
            _usuarioRepository = usuarioRepository;
            _envioEmailService = envioEmailService;
        }

        public async Task<(bool Sucesso, string Mensagem, int StatusCode)> SolicitarAlteracaoAsync(int usuarioId, string novoEmail)
        {
            novoEmail = (novoEmail ?? string.Empty).Trim().ToLowerInvariant();

            var usuario = await _usuarioRepository.ObterParaEdicaoPorIdAsync(usuarioId);
            if (usuario == null)
                return (false, "Usuario autenticado nao encontrado.", StatusCodes.Status401Unauthorized);

            if (string.Equals(usuario.Email, novoEmail, StringComparison.OrdinalIgnoreCase))
                return (false, "O novo email deve ser diferente do email atual.", StatusCodes.Status400BadRequest);

            if (await _usuarioRepository.ExisteEmailEmOutroUsuarioAsync(novoEmail, usuario.ID))
                return (false, "O email informado ja esta em uso.", StatusCodes.Status400BadRequest);

            var codigo = CadastroService.GerarCodigoVerificacao();
            var expiracao = DateTime.UtcNow.AddHours(24);

            try
            {
                await _envioEmailService.EnviarCodigoAlteracaoEmailAsync(novoEmail, codigo, expiresAt: expiracao);
            }
            catch
            {
                return (false, "Nao foi possivel enviar o codigo de confirmacao no momento.", StatusCodes.Status500InternalServerError);
            }

            await _usuarioRepository.AtualizarSolicitacaoAlteracaoEmailAsync(
                usuario.ID,
                novoEmail,
                codigo,
                expiracao);

            return (true, "Codigo de confirmacao enviado para o novo email.", StatusCodes.Status200OK);
        }

        public async Task<(bool Sucesso, string Mensagem, int StatusCode)> ConfirmarAlteracaoAsync(int usuarioId, string codigo)
        {
            codigo = (codigo ?? string.Empty).Trim();

            var usuario = await _usuarioRepository.ObterParaEdicaoPorIdAsync(usuarioId);
            if (usuario == null)
                return (false, "Usuario autenticado nao encontrado.", StatusCodes.Status401Unauthorized);

            if (string.IsNullOrWhiteSpace(usuario.EmailPendente))
                return (false, "Nao existe alteracao de email pendente para este usuario.", StatusCodes.Status400BadRequest);

            if (usuario.CodigoVerificacaoExpiracao is null || usuario.CodigoVerificacaoExpiracao < DateTime.UtcNow)
                return (false, "O codigo de confirmacao expirou.", StatusCodes.Status400BadRequest);

            if (!string.Equals(usuario.CodigoVerificacao, codigo, StringComparison.Ordinal))
                return (false, "O codigo de confirmacao informado e invalido.", StatusCodes.Status400BadRequest);

            if (await _usuarioRepository.ExisteEmailEmOutroUsuarioAsync(usuario.EmailPendente, usuario.ID))
                return (false, "O novo email informado ja esta em uso.", StatusCodes.Status400BadRequest);

            await _usuarioRepository.ConfirmarAlteracaoEmailAsync(usuario.ID, usuario.EmailPendente);

            return (true, "Email alterado com sucesso.", StatusCodes.Status200OK);
        }
    }
}
