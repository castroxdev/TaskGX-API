using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TaskGX.API.Models;
using TaskGX.API.Repositories;

namespace TaskGX.API.Services
{
    public class RecuperacaoSenhaService
    {
        private const int MinutosValidadeCodigo = 15;
        private const int MaximoTentativasInvalidas = 5;
        private const string MensagemSolicitacao =
            "Se o email estiver associado a uma conta, sera enviado um codigo de recuperacao.";
        private const string MensagemCodigoInvalido =
            "O codigo de recuperacao e invalido ou expirou.";

        private readonly UsuarioRepository _usuarioRepository;
        private readonly RecuperacaoSenhaRepository _recuperacaoRepository;
        private readonly EnvioEmailService _envioEmailService;
        private readonly ILogger<RecuperacaoSenhaService> _registrador;
        private readonly byte[] _chaveHmac;

        public RecuperacaoSenhaService(
            UsuarioRepository usuarioRepository,
            RecuperacaoSenhaRepository recuperacaoRepository,
            EnvioEmailService envioEmailService,
            IOptions<ConfiguracoesJwt> configuracoesJwt,
            ILogger<RecuperacaoSenhaService> registrador)
        {
            _usuarioRepository = usuarioRepository;
            _recuperacaoRepository = recuperacaoRepository;
            _envioEmailService = envioEmailService;
            _registrador = registrador;
            _chaveHmac = Encoding.UTF8.GetBytes(configuracoesJwt.Value.Chave);
        }

        public async Task<(bool Sucesso, string Mensagem, int StatusCode)> SolicitarAsync(string email)
        {
            email = NormalizarEmail(email);

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return (true, MensagemSolicitacao, StatusCodes.Status200OK);

            var codigo = GerarCodigo();
            var instanteAtual = DateTime.UtcNow;
            var expiracao = instanteAtual.AddMinutes(MinutosValidadeCodigo);
            var codigoHash = CalcularCodigoHash(usuario.ID, codigo);

            await _recuperacaoRepository.CriarOuSubstituirAsync(new RecuperacaoSenha
            {
                UsuarioID = usuario.ID,
                CodigoHash = codigoHash,
                Expiracao = expiracao,
                TentativasInvalidas = 0,
                CriadoEm = instanteAtual
            });

            try
            {
                await _envioEmailService.EnviarCodigoRecuperacaoSenhaAsync(
                    usuario.Email,
                    codigo,
                    expiracao);
            }
            catch (Exception ex)
            {
                await _recuperacaoRepository.RemoverSeCorresponderAsync(usuario.ID, codigoHash);
                _registrador.LogError(
                    ex,
                    "Falha ao enviar codigo de recuperacao de senha para {Email}.",
                    usuario.Email);
            }

            return (true, MensagemSolicitacao, StatusCodes.Status200OK);
        }

        public async Task<(bool Sucesso, string Mensagem, int StatusCode)> ValidarCodigoAsync(
            string email,
            string codigo)
        {
            email = NormalizarEmail(email);
            codigo = (codigo ?? string.Empty).Trim();

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return CodigoInvalido();

            var recuperacao = await _recuperacaoRepository.ObterPorUsuarioIdAsync(usuario.ID);
            if (recuperacao == null)
                return CodigoInvalido();

            if (recuperacao.Expiracao < DateTime.UtcNow ||
                recuperacao.TentativasInvalidas >= MaximoTentativasInvalidas)
            {
                await _recuperacaoRepository.RemoverSeCorresponderAsync(
                    usuario.ID,
                    recuperacao.CodigoHash);

                return CodigoInvalido();
            }

            if (!CodigoConfere(usuario.ID, codigo, recuperacao.CodigoHash))
            {
                await _recuperacaoRepository.RegistrarTentativaInvalidaAsync(
                    usuario.ID,
                    recuperacao.CodigoHash,
                    MaximoTentativasInvalidas);

                return CodigoInvalido();
            }

            return (
                true,
                "Codigo de recuperacao valido.",
                StatusCodes.Status200OK);
        }

        public async Task<(bool Sucesso, string Mensagem, int StatusCode)> RedefinirAsync(
            string email,
            string codigo,
            string novaSenha,
            string confirmarNovaSenha)
        {
            if (!string.Equals(novaSenha, confirmarNovaSenha, StringComparison.Ordinal))
            {
                return (
                    false,
                    "A confirmacao da nova senha nao confere.",
                    StatusCodes.Status400BadRequest);
            }

            if (!SenhaService.EhValida(novaSenha))
            {
                return (
                    false,
                    "A nova senha nao atende aos requisitos de seguranca.",
                    StatusCodes.Status400BadRequest);
            }

            email = NormalizarEmail(email);
            codigo = (codigo ?? string.Empty).Trim();

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return CodigoInvalido();

            var recuperacao = await _recuperacaoRepository.ObterPorUsuarioIdAsync(usuario.ID);
            if (recuperacao == null)
                return CodigoInvalido();

            if (recuperacao.Expiracao < DateTime.UtcNow ||
                recuperacao.TentativasInvalidas >= MaximoTentativasInvalidas)
            {
                await _recuperacaoRepository.RemoverSeCorresponderAsync(
                    usuario.ID,
                    recuperacao.CodigoHash);

                return CodigoInvalido();
            }

            if (!CodigoConfere(usuario.ID, codigo, recuperacao.CodigoHash))
            {
                await _recuperacaoRepository.RegistrarTentativaInvalidaAsync(
                    usuario.ID,
                    recuperacao.CodigoHash,
                    MaximoTentativasInvalidas);

                return CodigoInvalido();
            }

            var senhaAtualizada = await _recuperacaoRepository.RedefinirSenhaAsync(
                usuario.ID,
                recuperacao.CodigoHash,
                DateTime.UtcNow,
                MaximoTentativasInvalidas,
                SenhaService.GerarHash(novaSenha));

            if (!senhaAtualizada)
                return CodigoInvalido();

            return (
                true,
                "Senha redefinida com sucesso.",
                StatusCodes.Status200OK);
        }

        private static string NormalizarEmail(string email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string GerarCodigo()
        {
            return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        }

        private string CalcularCodigoHash(int usuarioId, string codigo)
        {
            var dados = Encoding.UTF8.GetBytes(
                $"TaskGX|recuperacao-senha|v1|{usuarioId}|{codigo}");

            using var hmac = new HMACSHA256(_chaveHmac);
            return Convert.ToBase64String(hmac.ComputeHash(dados));
        }

        private bool CodigoConfere(int usuarioId, string codigo, string codigoHashArmazenado)
        {
            try
            {
                var hashInformado = Convert.FromBase64String(CalcularCodigoHash(usuarioId, codigo));
                var hashArmazenado = Convert.FromBase64String(codigoHashArmazenado);

                return CryptographicOperations.FixedTimeEquals(hashInformado, hashArmazenado);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static (bool Sucesso, string Mensagem, int StatusCode) CodigoInvalido()
        {
            return (false, MensagemCodigoInvalido, StatusCodes.Status400BadRequest);
        }
    }
}
