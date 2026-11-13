using System.ComponentModel.DataAnnotations;

namespace PlayerasJyC.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(100)]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El correo es obligatorio."), EmailAddress(ErrorMessage = "Correo no válido."), StringLength(100)]
        public string Email { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(20)]
        public string Rol { get; set; } = "Cliente";

        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
