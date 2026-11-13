using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Módulo para visualizar y atender las compras (pedidos) de los clientes
    public class AdminPedidosController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminPedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index(string? estado)
        {
            var query = _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado) && Pedido.Estados.Contains(estado))
                query = query.Where(p => p.EstadoPedido == estado);

            ViewBag.Estado = estado;
            return View(query.OrderByDescending(p => p.FechaHora).ToList());
        }

        public IActionResult Detalle(int id)
        {
            var pedido = _context.PedidoCompleto(id);
            if (pedido == null) return NotFound();
            return View(pedido);
        }

        public IActionResult Ticket(int id)
        {
            var pedido = _context.PedidoCompleto(id);
            if (pedido == null) return NotFound();
            ViewBag.VolverController = "AdminPedidos";
            ViewBag.VolverAction = "Detalle";
            ViewBag.PuedeEnviar = false;
            return View("~/Views/Pedidos/Ticket.cshtml", pedido);
        }

        [HttpPost]
        public IActionResult CambiarEstado(int id, string estado, string? comentario)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Detalles)
                .Include(p => p.Pagos)
                .Include(p => p.Historial)
                .FirstOrDefault(p => p.PedidoId == id);
            if (pedido == null) return NotFound();

            if (!Pedido.Estados.Contains(estado))
            {
                TempData["Error"] = "Estado no válido.";
                return RedirectToAction(nameof(Detalle), new { id });
            }
            if (pedido.EstadoPedido == estado)
            {
                TempData["Error"] = "El pedido ya está en ese estado.";
                return RedirectToAction(nameof(Detalle), new { id });
            }
            if (pedido.EstadoPedido == "Cancelado")
            {
                TempData["Error"] = "Un pedido cancelado no se puede reactivar.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            // Al cancelar: regresar piezas al inventario y marcar el pago como reembolsado
            if (estado == "Cancelado")
            {
                var ids = pedido.Detalles.Select(d => d.VarianteId).ToList();
                var variantes = _context.Variantes.Where(v => ids.Contains(v.VarianteId)).ToList();
                foreach (var d in pedido.Detalles)
                {
                    var v = variantes.FirstOrDefault(x => x.VarianteId == d.VarianteId);
                    if (v != null) v.Stock += d.Cantidad;
                }
                foreach (var pg in pedido.Pagos) pg.EstadoPago = "Reembolsado";
            }

            comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
            if (comentario != null && comentario.Length > 250) comentario = comentario.Substring(0, 250);

            pedido.EstadoPedido = estado;
            pedido.Historial.Add(new HistorialEstado
            {
                Estado = estado,
                Comentario = comentario,
                RealizadoPor = HttpContext.Session.GetString("Nombre"),
                Fecha = DateTime.Now
            });

            _context.SaveChanges();
            TempData["Mensaje"] = "Estado actualizado a \"" + estado + "\".";
            return RedirectToAction(nameof(Detalle), new { id });
        }
    }
}
