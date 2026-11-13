namespace PlayerasJyC.Models
{
    public class ProductoDetalleVM
    {
        public Producto Producto { get; set; } = null!;
        public List<Variante> Variantes { get; set; } = new();
        public List<Resena> Resenas { get; set; } = new();
        public double Promedio { get; set; }
        public Resena? MiResena { get; set; }
        public bool EsCliente { get; set; }
    }

    public class MiCuentaVM
    {
        public Usuario Usuario { get; set; } = null!;
        public List<Direccion> Direcciones { get; set; } = new();
    }

    public class CheckoutVM
    {
        public List<CarritoItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public string? CuponCodigo { get; set; }
        public string? CuponMensaje { get; set; }
        public bool CuponValido { get; set; }
        public List<MetodoEnvio> Envios { get; set; } = new();
        public List<Direccion> Direcciones { get; set; } = new();
        public Direccion? Prefill { get; set; }
        public string? NombreUsuario { get; set; }
        public string? TelefonoUsuario { get; set; }
    }
}
