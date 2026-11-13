using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;
using PlayerasJyC.Services;

namespace PlayerasJyC.Controllers
{
    // Finalizar la compra: datos de envío, cupón, método de pago y generación del pedido
    public class CheckoutController : BaseController
    {
        private static readonly string[] MetodosPago = { "Tarjeta", "Transferencia", "Depósito OXXO" };

        private readonly ApplicationDbContext _context;
        private readonly CorreoService _correo;

        public CheckoutController(ApplicationDbContext context, CorreoService correo)
        {
            _context = context;
            _correo = correo;
        }

        protected override string? RolRequerido => "Cliente";

        private List<CarritoItem> CargarCarrito(int uid)
        {
            return _context.CarritoItems
                .Where(c => c.UsuarioId == uid)
                .Include(c => c.Variante).ThenInclude(v => v.Producto)
                .Include(c => c.Variante).ThenInclude(v => v.Talla)
                .Include(c => c.Variante).ThenInclude(v => v.Color)
                .OrderBy(c => c.FechaAgregado)
                .ToList();
        }

        public IActionResult Index(string? cupon)
        {
            int uid = UsuarioId!.Value;
            var items = CargarCarrito(uid);
            if (items.Count == 0)
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction("Index", "Carrito");
            }

            decimal subtotal = items.Sum(i => i.Cantidad * i.Variante.Producto.PrecioActual);

            var vm = new CheckoutVM
            {
                Items = items,
                Subtotal = subtotal,
                Envios = _context.MetodosEnvio.AsNoTracking().Where(e => e.Activo).OrderBy(e => e.Costo).ToList(),
                Direcciones = _context.Direcciones.AsNoTracking().Where(d => d.UsuarioId == uid)
                    .OrderByDescending(d => d.Predeterminada).ThenBy(d => d.Alias).ToList()
            };
            vm.Prefill = vm.Direcciones.FirstOrDefault();

            var usuario = _context.Usuarios.AsNoTracking().FirstOrDefault(u => u.UsuarioId == uid);
            vm.NombreUsuario = usuario?.Nombre;
            vm.TelefonoUsuario = usuario?.Telefono;

            if (!string.IsNullOrWhiteSpace(cupon))
            {
                string codigo = cupon.Trim().ToUpperInvariant();
                var cp = _context.Cupones.AsNoTracking().FirstOrDefault(c => c.Codigo == codigo);
                string? msg = cp == null ? "El cupón no existe." : cp.Validar(subtotal);
                if (msg == null && cp != null)
                {
                    vm.CuponValido = true;
                    vm.CuponCodigo = cp.Codigo;
                    vm.Descuento = cp.Descuento(subtotal);
                    vm.CuponMensaje = "Cupón aplicado: -$" + vm.Descuento.ToString("N2");
                }
                else
                {
                    vm.CuponCodigo = codigo;
                    vm.CuponMensaje = msg;
                }
            }

            return View(vm);
        }

        [HttpPost]
        public IActionResult Confirmar(string nombreDestinatario, string calle, string? colonia, string ciudad, string estado,
            string codigoPostal, string? telefono, bool guardarDireccion, int metodoEnvioId, string metodoPago, string? cupon)
        {
            int uid = UsuarioId!.Value;
            var items = CargarCarrito(uid);
            if (items.Count == 0)
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction("Index", "Carrito");
            }

            nombreDestinatario = (nombreDestinatario ?? "").Trim();
            calle = (calle ?? "").Trim();
            colonia = string.IsNullOrWhiteSpace(colonia) ? null : colonia.Trim();
            ciudad = (ciudad ?? "").Trim();
            estado = (estado ?? "").Trim();
            codigoPostal = (codigoPostal ?? "").Trim();
            telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();

            var envio = _context.MetodosEnvio.FirstOrDefault(e => e.MetodoEnvioId == metodoEnvioId && e.Activo);

            string? error = null;
            if (nombreDestinatario.Length < 3 || nombreDestinatario.Length > 100) error = "Escribe el nombre de quien recibe.";
            else if (calle.Length < 3 || calle.Length > 150) error = "Escribe la calle y número.";
            else if (ciudad.Length < 2 || ciudad.Length > 100) error = "Escribe la ciudad.";
            else if (estado.Length < 2 || estado.Length > 100) error = "Escribe el estado.";
            else if (!System.Text.RegularExpressions.Regex.IsMatch(codigoPostal, @"^\d{5}$")) error = "El código postal debe tener 5 dígitos.";
            else if (telefono != null && telefono.Length > 20) error = "El teléfono es demasiado largo.";
            else if (envio == null) error = "Elige un método de envío.";
            else if (!MetodosPago.Contains(metodoPago)) error = "Elige un método de pago.";

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index), new { cupon });
            }

            // Verificar stock y que los productos sigan disponibles
            foreach (var it in items)
            {
                var v = it.Variante;
                if (!v.Producto.Activo || it.Cantidad > v.Stock)
                {
                    TempData["Error"] = "Ya no hay suficiente stock de " + v.Producto.Nombre + " (" + v.Talla.Nombre + " / " + v.Color.Nombre + "). Disponible: " + v.Stock + ". Ajusta tu carrito.";
                    return RedirectToAction("Index", "Carrito");
                }
            }

            decimal subtotal = items.Sum(i => i.Cantidad * i.Variante.Producto.PrecioActual);

            // Cupón (opcional)
            Cupon? cp = null;
            decimal descuento = 0m;
            if (!string.IsNullOrWhiteSpace(cupon))
            {
                string codigo = cupon.Trim().ToUpperInvariant();
                cp = _context.Cupones.FirstOrDefault(c => c.Codigo == codigo);
                string? msg = cp == null ? "El cupón no existe." : cp.Validar(subtotal);
                if (msg != null || cp == null)
                {
                    TempData["Error"] = msg ?? "El cupón no existe.";
                    return RedirectToAction(nameof(Index), new { cupon });
                }
                descuento = cp.Descuento(subtotal);
                cp.UsosActuales += 1;
            }

            decimal total = subtotal - descuento + envio!.Costo;
            if (total < 0) total = 0;

            string direccionTexto = calle + (colonia != null ? ", " + colonia : "") + ", " + estado;

            var pedido = new Pedido
            {
                UsuarioId = uid,
                FechaHora = DateTime.Now,
                Subtotal = subtotal,
                Descuento = descuento,
                CostosEnvio = envio.Costo,
                Total = total,
                EstadoPedido = "Pagado",
                MetodoPago = metodoPago,
                MetodoEnvioId = envio.MetodoEnvioId,
                CuponId = cp?.CuponId,
                NombreDestinatario = nombreDestinatario,
                DireccionEnvio = direccionTexto.Length > 250 ? direccionTexto.Substring(0, 250) : direccionTexto,
                Ciudad = ciudad,
                CodigoPostal = codigoPostal,
                Telefono = telefono
            };

            foreach (var it in items)
            {
                var v = it.Variante;
                pedido.Detalles.Add(new DetallePedido
                {
                    ProductoId = v.ProductoId,
                    VarianteId = v.VarianteId,
                    NombreProducto = v.Producto.Nombre,
                    Talla = v.Talla.Nombre,
                    Color = v.Color.Nombre,
                    Cantidad = it.Cantidad,
                    PrecioUnitario = v.Producto.PrecioActual
                });
                v.Stock -= it.Cantidad;
            }

            // Pago simulado (aprobado al instante)
            pedido.Pagos.Add(new Pago
            {
                MetodoPago = metodoPago,
                Monto = total,
                EstadoPago = "Aprobado",
                Referencia = "PJ" + DateTime.Now.ToString("yyMMddHHmmss") + Random.Shared.Next(100, 999),
                FechaPago = DateTime.Now
            });

            pedido.Historial.Add(new HistorialEstado
            {
                Estado = "Pagado",
                Comentario = "Pedido recibido y pago aprobado.",
                RealizadoPor = "Sistema",
                Fecha = DateTime.Now
            });

            _context.Pedidos.Add(pedido);
            _context.CarritoItems.RemoveRange(items);

            if (guardarDireccion)
            {
                bool primera = !_context.Direcciones.Any(d => d.UsuarioId == uid);
                _context.Direcciones.Add(new Direccion
                {
                    UsuarioId = uid,
                    Alias = "Casa",
                    NombreDestinatario = nombreDestinatario,
                    Calle = calle,
                    Colonia = colonia,
                    Ciudad = ciudad,
                    Estado = estado,
                    CodigoPostal = codigoPostal,
                    Telefono = telefono,
                    Predeterminada = primera
                });
            }

            using (var tx = _context.Database.BeginTransaction())
            {
                _context.SaveChanges();
                tx.Commit();
            }

            GuardarContadorCarrito(0);

            // Ticket por correo (si el correo del sistema está configurado)
            bool enviado = false;
            var completo = _context.PedidoCompleto(pedido.PedidoId);
            if (completo != null) enviado = _correo.EnviarTicket(completo);

            TempData["Mensaje"] = enviado
                ? "¡Compra realizada! Te enviamos el ticket a tu correo."
                : "¡Compra realizada! Aquí está tu ticket.";
            return RedirectToAction("Ticket", "Pedidos", new { id = pedido.PedidoId });
        }
    }
}
