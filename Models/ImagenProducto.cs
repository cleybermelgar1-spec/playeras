using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class ImagenProducto
    {
        public int ImagenProductoId { get; set; }
        public int ProductoId { get; set; }

        [Required, StringLength(300)]
        public string Url { get; set; } = "";

        public int Orden { get; set; }

        public Producto Producto { get; set; } = null!;
    }
}
