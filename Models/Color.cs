using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Color
    {
        public int ColorId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(40)]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El código de color es obligatorio."), StringLength(7)]
        public string CodigoHex { get; set; } = "#000000";
    }
}
