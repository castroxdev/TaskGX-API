using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.Services
{
    public class ConfiguracoesGoogleAuth
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;
    }
}
