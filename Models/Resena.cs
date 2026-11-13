using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Resena
    {
        public int ResenaId { get; set; }
        public int UsuarioId { get; set; }
        public int ProductoId { get; set; }

        [Range(1, 5)] public int Calificacion { get; set; } = 5;
        [StringLength(500)] public string? Comentario { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public bool Visible { get; set; } = true;

        public Usuario Usuario { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
    }
}
