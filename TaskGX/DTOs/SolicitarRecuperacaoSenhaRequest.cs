using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class SolicitarRecuperacaoSenhaRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
    }
}
