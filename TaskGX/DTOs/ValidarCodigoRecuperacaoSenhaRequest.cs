using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class ValidarCodigoRecuperacaoSenhaRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "O codigo deve conter 6 digitos.")]
        public string Codigo { get; set; } = string.Empty;
    }
}
