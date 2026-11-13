using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Variante
    {
        public int VarianteId { get; set; }
        public int ProductoId { get; set; }
        public int TallaId { get; set; }
        public int ColorId { get; set; }

        [StringLength(40)]
        public string? Sku { get; set; }

        public int Stock { get; set; }

        public Producto Producto { get; set; } = null!;
        public Talla Talla { get; set; } = null!;
        public Color Color { get; set; } = null!;
    }
}
