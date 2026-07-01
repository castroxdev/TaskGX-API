using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskGX.API.Models
{
    [Table("RecuperacoesSenha")]
    public class RecuperacaoSenha
    {
        [Key]
        [Column("UsuarioID")]
        public int UsuarioID { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("CodigoHash")]
        public string CodigoHash { get; set; } = string.Empty;

        [Column("Expiracao")]
        public DateTime Expiracao { get; set; }

        [Column("TentativasInvalidas")]
        public int TentativasInvalidas { get; set; }

        [Column("CriadoEm")]
        public DateTime CriadoEm { get; set; }
    }
}
