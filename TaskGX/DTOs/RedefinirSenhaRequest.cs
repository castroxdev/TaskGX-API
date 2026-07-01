using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class RedefinirSenhaRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "O codigo deve conter 6 digitos.")]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string NovaSenha { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        [Compare(nameof(NovaSenha), ErrorMessage = "A confirmacao da nova senha nao confere.")]
        public string ConfirmarNovaSenha { get; set; } = string.Empty;
    }
}
