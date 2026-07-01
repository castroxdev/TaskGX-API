using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace TaskGX.API.Services
{
    public class EnvioEmailService
    {
        private readonly ConfiguracoesEmail _configuracoes;
        private readonly ILogger<EnvioEmailService> _registrador;

        public EnvioEmailService(IOptions<ConfiguracoesEmail> configuracoes, ILogger<EnvioEmailService> registrador)
        {
            _configuracoes = configuracoes.Value;
            _registrador = registrador;
        }

        public async Task EnviarAsync(string emailDestino, string assunto, string corpoHtml, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(emailDestino))
                throw new ArgumentException("emailDestino e obrigatorio.", nameof(emailDestino));

            if (string.IsNullOrWhiteSpace(assunto))
                throw new ArgumentException("assunto e obrigatorio.", nameof(assunto));

            if (string.IsNullOrWhiteSpace(corpoHtml))
                throw new ArgumentException("corpoHtml e obrigatorio.", nameof(corpoHtml));

            if (string.IsNullOrWhiteSpace(_configuracoes.Host))
                throw new InvalidOperationException("ConfiguracoesEmail.Host nao configurado.");

            if (_configuracoes.Porta <= 0)
                throw new InvalidOperationException("ConfiguracoesEmail.Porta invalida.");

            if (string.IsNullOrWhiteSpace(_configuracoes.NomeUsuario))
                throw new InvalidOperationException("ConfiguracoesEmail.NomeUsuario nao configurado.");

            if (string.IsNullOrWhiteSpace(_configuracoes.Senha))
                throw new InvalidOperationException("ConfiguracoesEmail.Senha nao configurada.");

            var emailRemetente = string.IsNullOrWhiteSpace(_configuracoes.EmailRemetente)
                ? _configuracoes.NomeUsuario
                : _configuracoes.EmailRemetente;

            using var mensagem = new MailMessage
            {
                From = new MailAddress(emailRemetente, _configuracoes.NomeRemetente),
                Subject = assunto,
                Body = corpoHtml,
                IsBodyHtml = true
            };

            mensagem.To.Add(new MailAddress(emailDestino));

            using var cliente = new SmtpClient(_configuracoes.Host, _configuracoes.Porta)
            {
                EnableSsl = _configuracoes.HabilitarSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_configuracoes.NomeUsuario, _configuracoes.Senha),
                Timeout = 10000
            };

            try
            {
                ct.ThrowIfCancellationRequested();
                await cliente.SendMailAsync(mensagem);
            }
            catch (SmtpException ex)
            {
                _registrador.LogError(ex, "Falha SMTP ao enviar email para {EmailDestino}. Host={Host} Porta={Porta}", emailDestino, _configuracoes.Host, _configuracoes.Porta);
                throw;
            }
            catch (Exception ex)
            {
                _registrador.LogError(ex, "Erro inesperado ao enviar email para {EmailDestino}", emailDestino);
                throw;
            }
        }

        public Task EnviarCodigoVerificacaoAsync(string emailDestino, string codigo, DateTime? expiresAt = null, CancellationToken ct = default)
        {
            var expiracao = expiresAt is null ? string.Empty : $"<p>Expira em: <b>{expiresAt:dd/MM/yyyy HH:mm}</b></p>";

            var assunto = "Seu codigo de verificacao - TaskGX";
            var corpo = $@"
                <div style='font-family: Arial, sans-serif; line-height:1.5'>
                    <h2>TaskGX</h2>
                    <p>Seu codigo de verificacao e:</p>
                    <p style='font-size: 22px; letter-spacing: 2px;'><b>{WebUtility.HtmlEncode(codigo)}</b></p>
                    {expiracao}
                    <p>Se voce nao solicitou isso, pode ignorar este email.</p>
                </div>";

            return EnviarAsync(emailDestino, assunto, corpo, ct);
        }

        public Task EnviarCodigoAlteracaoEmailAsync(string emailDestino, string codigo, DateTime? expiresAt = null, CancellationToken ct = default)
        {
            var expiracao = expiresAt is null ? string.Empty : $"<p>Expira em: <b>{expiresAt:dd/MM/yyyy HH:mm}</b></p>";

            var assunto = "Confirmacao de alteracao de email - TaskGX";
            var corpo = $@"
                <div style='font-family: Arial, sans-serif; line-height:1.5'>
                    <h2>TaskGX</h2>
                    <p>Use o codigo abaixo para confirmar a alteracao do seu email:</p>
                    <p style='font-size: 22px; letter-spacing: 2px;'><b>{WebUtility.HtmlEncode(codigo)}</b></p>
                    {expiracao}
                    <p>Se voce nao solicitou essa alteracao, ignore este email.</p>
                </div>";

            return EnviarAsync(emailDestino, assunto, corpo, ct);
        }

        public Task EnviarCodigoRecuperacaoSenhaAsync(
            string emailDestino,
            string codigo,
            DateTime expiresAt,
            CancellationToken ct = default)
        {
            var assunto = "Recuperacao de senha - TaskGX";
            var corpo = $@"
                <div style='font-family: Arial, sans-serif; line-height:1.5'>
                    <h2>TaskGX</h2>
                    <p>Use o codigo abaixo para recuperar a sua senha:</p>
                    <p style='font-size: 22px; letter-spacing: 2px;'><b>{WebUtility.HtmlEncode(codigo)}</b></p>
                    <p>O codigo expira em: <b>{expiresAt:dd/MM/yyyy HH:mm}</b></p>
                    <p>Se voce nao solicitou a recuperacao, ignore este email.</p>
                </div>";

            return EnviarAsync(emailDestino, assunto, corpo, ct);
        }
    }
}
