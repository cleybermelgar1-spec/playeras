using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Pago
    {
        public int PagoId { get; set; }
        public int PedidoId { get; set; }

        [StringLength(40)] public string MetodoPago { get; set; } = "";
        public decimal Monto { get; set; }
        [StringLength(20)] public string EstadoPago { get; set; } = "Aprobado";
        [StringLength(50)] public string? Referencia { get; set; }
        public DateTime FechaPago { get; set; } = DateTime.Now;

        public Pedido Pedido { get; set; } = null!;
    }
}
