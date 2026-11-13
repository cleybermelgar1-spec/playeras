using System;
using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class CarritoCompra
    {
        [Key]
        public int CarritoId { get; set; }
        public int UsuarioId { get; set; }
        public int ProductoId { get; set; }

        public int Cantidad { get; set; } = 1;

        [Required]
        public string Talla { get; set; }

        public DateTime FechaAgregado { get; set; } = DateTime.Now;

        // Relaciones
        public Usuario Usuario { get; set; }
        public Producto Producto { get; set; }
    }
}