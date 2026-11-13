using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class MetodoEnvio
    {
        public int MetodoEnvioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(60)]
        public string Nombre { get; set; } = "";

        public decimal Costo { get; set; }
        public int DiasEstimados { get; set; } = 3;
        public bool Activo { get; set; } = true;
    }
}
