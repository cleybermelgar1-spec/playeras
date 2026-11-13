using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayerasJyC.Models
{
    public class DetallePedido
    {
        public int DetallePedidoId { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int VarianteId { get; set; }

        [StringLength(100)] public string NombreProducto { get; set; } = "";
        [StringLength(10)] public string Talla { get; set; } = "";
        [StringLength(40)] public string Color { get; set; } = "";

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Calculado por la base de datos: Cantidad * PrecioUnitario
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Importe { get; private set; }

        public Pedido Pedido { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
        public Variante Variante { get; set; } = null!;
    }
}
