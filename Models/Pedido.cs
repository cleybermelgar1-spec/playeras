using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayerasJyC.Models
{
    public class Pedido
    {
        public static readonly string[] Estados = { "Pagado", "Preparando", "Enviado", "Entregado", "Cancelado" };

        public int PedidoId { get; set; }
        public int UsuarioId { get; set; }

        // Folio generado por la base de datos: PJ-000001
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string Folio { get; private set; } = "";

        public DateTime FechaHora { get; set; } = DateTime.Now;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal CostosEnvio { get; set; }
        public decimal Total { get; set; }

        [StringLength(20)] public string EstadoPedido { get; set; } = "Pagado";
        [StringLength(40)] public string MetodoPago { get; set; } = "";

        public int? MetodoEnvioId { get; set; }
        public int? CuponId { get; set; }

        [StringLength(100)] public string NombreDestinatario { get; set; } = "";
        [StringLength(250)] public string DireccionEnvio { get; set; } = "";
        [StringLength(100)] public string Ciudad { get; set; } = "";
        [StringLength(10)] public string CodigoPostal { get; set; } = "";
        [StringLength(20)] public string? Telefono { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public MetodoEnvio? MetodoEnvio { get; set; }
        public Cupon? Cupon { get; set; }
        public List<DetallePedido> Detalles { get; set; } = new();
        public List<Pago> Pagos { get; set; } = new();
        public List<HistorialEstado> Historial { get; set; } = new();
    }
}
