using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Cupon
    {
        public int CuponId { get; set; }

        [Required(ErrorMessage = "El código es obligatorio."), StringLength(30)]
        public string Codigo { get; set; } = "";

        [StringLength(150)]
        public string? Descripcion { get; set; }

        // "Porcentaje" o "Monto"
        [StringLength(12)]
        public string Tipo { get; set; } = "Porcentaje";

        [Range(0.01, 100000, ErrorMessage = "Escribe un valor válido.")]
        public decimal Valor { get; set; }

        public decimal MinimoCompra { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Today;
        public DateTime FechaFin { get; set; } = DateTime.Today.AddMonths(1);
        public int? UsosMaximos { get; set; }
        public int UsosActuales { get; set; }
        public bool Activo { get; set; } = true;

        // Devuelve un mensaje de error si el cupón no se puede usar; null si es válido.
        public string? Validar(decimal subtotal)
        {
            var hoy = DateTime.Today;
            if (!Activo) return "El cupón no está activo.";
            if (hoy < FechaInicio.Date || hoy > FechaFin.Date) return "El cupón no está vigente.";
            if (UsosMaximos.HasValue && UsosActuales >= UsosMaximos.Value) return "El cupón ya alcanzó su límite de usos.";
            if (subtotal < MinimoCompra) return "El cupón requiere una compra mínima de $" + MinimoCompra.ToString("N2") + ".";
            return null;
        }

        public decimal Descuento(decimal subtotal)
        {
            decimal d = Tipo == "Porcentaje" ? subtotal * Valor / 100m : Valor;
            if (d > subtotal) d = subtotal;
            return Math.Round(d, 2);
        }
    }
}
