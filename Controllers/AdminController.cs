using Microsoft.AspNetCore.Mvc;
using PlayerasJyC.Data;

namespace PlayerasJyC.Controllers
{
    // Panel de administración y dashboard con estadísticas (solo administradores)
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            // Contadores
            ViewBag.Clientes = _context.Usuarios.Count(u => u.Rol == "Cliente");
            ViewBag.Productos = _context.Productos.Count(p => p.Activo);
            ViewBag.Pedidos = _context.Pedidos.Count();
            ViewBag.PedidosPendientes = _context.Pedidos.Count(p => p.EstadoPedido == "Pagado" || p.EstadoPedido == "Preparando");
            ViewBag.Ventas = _context.Pedidos.Where(p => p.EstadoPedido != "Cancelado").Sum(p => (decimal?)p.Total) ?? 0m;
            ViewBag.StockBajo = _context.Variantes.Count(v => v.Stock <= 5);
            ViewBag.SolicitudesNuevas = _context.SolicitudesContacto.Count(s => !s.Atendida);
            ViewBag.Resenas = _context.Resenas.Count();

            // Gráfica 1: pedidos por estado
            ViewBag.PorEstado = _context.Pedidos
                .GroupBy(p => p.EstadoPedido)
                .Select(g => new { nombre = g.Key, total = g.Count() })
                .ToList();

            // Gráfica 2: productos más vendidos (piezas)
            ViewBag.TopProductos = _context.DetallePedidos
                .Where(d => d.Pedido.EstadoPedido != "Cancelado")
                .GroupBy(d => d.NombreProducto)
                .Select(g => new { nombre = g.Key, total = g.Sum(x => x.Cantidad) })
                .OrderByDescending(x => x.total)
                .Take(5)
                .ToList();

            // Gráfica 3: ventas por categoría
            ViewBag.VentasCategoria = _context.DetallePedidos
                .Where(d => d.Pedido.EstadoPedido != "Cancelado")
                .GroupBy(d => d.Producto.Categoria.Nombre)
                .Select(g => new { nombre = g.Key, total = g.Sum(x => x.Importe) })
                .ToList();

            // Gráfica 4: ventas por mes (últimos 6 meses con ventas)
            var meses = _context.Pedidos
                .Where(p => p.EstadoPedido != "Cancelado")
                .GroupBy(p => new { p.FechaHora.Year, p.FechaHora.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Total) })
                .ToList();
            ViewBag.VentasMes = meses
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .TakeLast(6)
                .Select(x => new { nombre = x.Month + "/" + x.Year, total = x.Total })
                .ToList();

            return View();
        }
    }
}
