using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Coleccion
    {
        public int ColeccionId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(80)]
        public string Nombre { get; set; } = "";

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
