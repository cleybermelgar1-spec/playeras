using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;
using PlayerasJyC.Services;

namespace PlayerasJyC.Controllers
{
    // "Mis pedidos": historial de compras del cliente, ticket y cancelación
    public class PedidosController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly CorreoService _correo;

        public PedidosController(ApplicationDbContext context, CorreoService correo)
        {
            _context = context;
            _correo = correo;
        }

        protected override string? RolRequerido => "Cliente";

        public IActionResult Index()
        {
            int uid = UsuarioId!.Value;
            var pedidos = _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Detalles)
                .Where(p => p.UsuarioId == uid)
                .OrderByDescending(p => p.FechaHora)
                .ToList();
            return View(pedidos);
        }

        private Pedido? MiPedido(int id)
        {
            var p = _context.PedidoCompleto(id);
            if (p == null || p.UsuarioId != UsuarioId) return null;
            return p;
        }

        public IActionResult Detalle(int id)
        {
            var pedido = MiPedido(id);
            if (pedido == null) return NotFound();
            return View(pedido);
        }

        // Ticket / comprobante de compra (imprimible o guardable en PDF)
        public IActionResult Ticket(int id)
        {
            var pedido = MiPedido(id);
            if (pedido == null) return NotFound();
            ViewBag.VolverController = "Pedidos";
            ViewBag.VolverAction = "Detalle";
            ViewBag.PuedeEnviar = true;
            return View(pedido);
        }

        [HttpPost]
        public IActionResult EnviarTicket(int id, string? correo)
        {
            var pedido = MiPedido(id);
            if (pedido == null) return NotFound();

            correo = string.IsNullOrWhiteSpace(correo) ? null : correo.Trim();
            if (correo != null && (correo.Length > 100 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(correo)))
            {
                TempData["Error"] = "Escribe un correo válido.";
                return RedirectToAction(nameof(Ticket), new { id });
            }

            bool ok = _correo.EnviarTicket(pedido, correo);
            if (ok) TempData["Mensaje"] = "Ticket enviado a " + (correo ?? pedido.Usuario.Email) + ".";
            else TempData["Error"] = "No se pudo enviar el correo. Puedes imprimir o guardar el ticket en PDF.";
            return RedirectToAction(nameof(Ticket), new { id });
        }

        // El cliente puede cancelar mientras el pedido siga en estado "Pagado"
        [HttpPost]
        public IActionResult Cancelar(int id)
        {
            int uid = UsuarioId!.Value;
            var pedido = _context.Pedidos
                .Include(p => p.Detalles)
                .Include(p => p.Pagos)
                .Include(p => p.Historial)
                .FirstOrDefault(p => p.PedidoId == id && p.UsuarioId == uid);
            if (pedido == null) return NotFound();

            if (pedido.EstadoPedido != "Pagado")
            {
                TempData["Error"] = "Este pedido ya está en proceso y no se puede cancelar desde aquí. Contáctanos.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            // Regresar las piezas al inventario
            var ids = pedido.Detalles.Select(d => d.VarianteId).ToList();
            var variantes = _context.Variantes.Where(v => ids.Contains(v.VarianteId)).ToList();
            foreach (var d in pedido.Detalles)
            {
                var v = variantes.FirstOrDefault(x => x.VarianteId == d.VarianteId);
                if (v != null) v.Stock += d.Cantidad;
            }

            pedido.EstadoPedido = "Cancelado";
            foreach (var pg in pedido.Pagos) pg.EstadoPago = "Reembolsado";
            pedido.Historial.Add(new HistorialEstado
            {
                Estado = "Cancelado",
                Comentario = "Cancelado por el cliente. Pago reembolsado.",
                RealizadoPor = HttpContext.Session.GetString("Nombre"),
                Fecha = DateTime.Now
            });

            _context.SaveChanges();
            TempData["Mensaje"] = "Pedido cancelado. Tu pago será reembolsado.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
    }
}
