using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayerasJyC.Models
{
    public class Producto
    {
        public int ProductoId { get; set; }
        public int CategoriaId { get; set; }
        public int? ColeccionId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(100)]
        public string Nombre { get; set; } = "";

        public string? Descripcion { get; set; }

        [StringLength(100)]
        public string? Material { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Escribe un precio válido.")]
        public decimal PrecioOriginal { get; set; }

        public bool EnOferta { get; set; }
        public decimal? PrecioOferta { get; set; }

        [StringLength(300)]
        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaAlta { get; set; } = DateTime.Now;

        public Categoria Categoria { get; set; } = null!;
        public Coleccion? Coleccion { get; set; }
        public List<Variante> Variantes { get; set; } = new();
        public List<ImagenProducto> Imagenes { get; set; } = new();
        public List<Resena> Resenas { get; set; } = new();

        [NotMapped]
        public decimal PrecioActual => EnOferta && PrecioOferta.HasValue ? PrecioOferta.Value : PrecioOriginal;

        [NotMapped]
        public int StockTotal => Variantes.Sum(v => v.Stock);

        [NotMapped]
        public string Imagen => string.IsNullOrWhiteSpace(ImagenUrl) ? "/images/sin-imagen.svg" : ImagenUrl;
    }
}
