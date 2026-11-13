using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Carrito de compras (solo clientes registrados)
    public class CarritoController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Cliente";

        public IActionResult Index()
        {
            int uid = UsuarioId!.Value;
            var items = _context.CarritoItems
                .AsNoTracking()
                .Where(c => c.UsuarioId == uid)
                .Include(c => c.Variante).ThenInclude(v => v.Producto)
                .Include(c => c.Variante).ThenInclude(v => v.Talla)
                .Include(c => c.Variante).ThenInclude(v => v.Color)
                .OrderBy(c => c.FechaAgregado)
                .ToList();
            return View(items);
        }

        [HttpPost]
        public IActionResult Agregar(int varianteId, int cantidad)
        {
            int uid = UsuarioId!.Value;
            if (cantidad < 1) cantidad = 1;

            var variante = _context.Variantes.Include(v => v.Producto).FirstOrDefault(v => v.VarianteId == varianteId);
            if (variante == null || !variante.Producto.Activo)
            {
                TempData["Error"] = "Elige una talla y un color disponibles.";
                return RedirectToAction("Index", "Productos");
            }

            var item = _context.CarritoItems.FirstOrDefault(c => c.UsuarioId == uid && c.VarianteId == varianteId);
            int yaEnCarrito = item?.Cantidad ?? 0;

            if (yaEnCarrito + cantidad > variante.Stock)
            {
                TempData["Error"] = "Solo hay " + variante.Stock + " pieza(s) disponibles de esa talla y color.";
                return RedirectToAction("Detalle", "Productos", new { id = variante.ProductoId });
            }

            if (item == null)
            {
                _context.CarritoItems.Add(new CarritoItem { UsuarioId = uid, VarianteId = varianteId, Cantidad = cantidad, FechaAgregado = DateTime.Now });
            }
            else
            {
                item.Cantidad += cantidad;
            }
            _context.SaveChanges();
            RefrescarContador(uid);

            TempData["Mensaje"] = "Producto agregado al carrito.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Actualizar(int id, int cantidad)
        {
            int uid = UsuarioId!.Value;
            var item = _context.CarritoItems.Include(c => c.Variante).FirstOrDefault(c => c.CarritoItemId == id && c.UsuarioId == uid);
            if (item == null) return RedirectToAction(nameof(Index));

            if (cantidad <= 0)
            {
                _context.CarritoItems.Remove(item);
            }
            else if (cantidad > item.Variante.Stock)
            {
                TempData["Error"] = "Solo hay " + item.Variante.Stock + " pieza(s) disponibles.";
                item.Cantidad = item.Variante.Stock > 0 ? item.Variante.Stock : 1;
            }
            else
            {
                item.Cantidad = cantidad;
            }

            _context.SaveChanges();
            RefrescarContador(uid);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Quitar(int id)
        {
            int uid = UsuarioId!.Value;
            var item = _context.CarritoItems.FirstOrDefault(c => c.CarritoItemId == id && c.UsuarioId == uid);
            if (item != null)
            {
                _context.CarritoItems.Remove(item);
                _context.SaveChanges();
                RefrescarContador(uid);
                TempData["Mensaje"] = "Producto quitado del carrito.";
            }
            return RedirectToAction(nameof(Index));
        }

        private void RefrescarContador(int uid)
        {
            int n = _context.CarritoItems.Where(c => c.UsuarioId == uid).Sum(c => (int?)c.Cantidad) ?? 0;
            GuardarContadorCarrito(n);
        }
    }
}
