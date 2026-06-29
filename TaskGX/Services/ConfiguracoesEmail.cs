using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.Services
{
    public class ConfiguracoesEmail
    {
        public string Host { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int Porta { get; set; } = 587;

        public string NomeUsuario { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;

        public string? EmailRemetente { get; set; }

        public string NomeRemetente { get; set; } = "TaskGX";

        public bool HabilitarSsl { get; set; } = true;
    }
}
