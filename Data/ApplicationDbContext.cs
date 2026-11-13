using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Models;

namespace PlayerasJyC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Direccion> Direcciones { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Coleccion> Colecciones { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ImagenProducto> ImagenesProducto { get; set; }
        public DbSet<Talla> Tallas { get; set; }
        public DbSet<Color> Colores { get; set; }
        public DbSet<Variante> Variantes { get; set; }
        public DbSet<CarritoItem> CarritoItems { get; set; }
        public DbSet<Cupon> Cupones { get; set; }
        public DbSet<MetodoEnvio> MetodosEnvio { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<HistorialEstado> HistorialEstados { get; set; }
        public DbSet<Resena> Resenas { get; set; }
        public DbSet<SolicitudContacto> SolicitudesContacto { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Todos los decimales (precios, totales) se guardan como decimal(18,2)
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }
    }
}
