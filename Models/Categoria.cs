using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Categoria
    {
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(50)]
        public string Nombre { get; set; } = "";

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
