using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class HistorialEstado
    {
        public int HistorialEstadoId { get; set; }
        public int PedidoId { get; set; }

        [StringLength(20)] public string Estado { get; set; } = "";
        [StringLength(250)] public string? Comentario { get; set; }
        [StringLength(100)] public string? RealizadoPor { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        public Pedido Pedido { get; set; } = null!;
    }
}
