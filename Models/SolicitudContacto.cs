using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class SolicitudContacto
    {
        [Key]
        public int SolicitudId { get; set; }

        [StringLength(100)] public string NombreCompleto { get; set; } = "";
        [StringLength(100)] public string Email { get; set; } = "";
        [StringLength(100)] public string PlayeraSolicitada { get; set; } = "";
        [StringLength(10)] public string Talla { get; set; } = "";
        [StringLength(500)] public string? Detalles { get; set; }
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
        public bool Atendida { get; set; }
    }
}
