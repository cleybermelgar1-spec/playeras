namespace PlayerasJyC.Models
{
    public class CarritoItem
    {
        public int CarritoItemId { get; set; }
        public int UsuarioId { get; set; }
        public int VarianteId { get; set; }
        public int Cantidad { get; set; } = 1;
        public DateTime FechaAgregado { get; set; } = DateTime.Now;

        public Usuario Usuario { get; set; } = null!;
        public Variante Variante { get; set; } = null!;
    }
}
