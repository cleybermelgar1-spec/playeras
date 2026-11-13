using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Talla
    {
        public int TallaId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(10)]
        public string Nombre { get; set; } = "";

        public int Orden { get; set; }
    }
}
