using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Direccion
    {
        public int DireccionId { get; set; }
        public int UsuarioId { get; set; }

        [StringLength(50)] public string Alias { get; set; } = "Casa";
        [StringLength(100)] public string NombreDestinatario { get; set; } = "";
        [StringLength(150)] public string Calle { get; set; } = "";
        [StringLength(100)] public string? Colonia { get; set; }
        [StringLength(100)] public string Ciudad { get; set; } = "";
        [StringLength(100)] public string Estado { get; set; } = "";
        [StringLength(10)] public string CodigoPostal { get; set; } = "";
        [StringLength(20)] public string? Telefono { get; set; }
        public bool Predeterminada { get; set; }

        public Usuario Usuario { get; set; } = null!;
    }
}
