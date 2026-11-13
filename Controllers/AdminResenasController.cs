using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;

namespace PlayerasJyC.Controllers
{
    // Moderación de reseñas de productos
    public class AdminResenasController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminResenasController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            var lista = _context.Resenas
                .AsNoTracking()
                .Include(r => r.Usuario)
                .Include(r => r.Producto)
                .OrderByDescending(r => r.Fecha)
                .ToList();
            return View(lista);
        }

        [HttpPost]
        public IActionResult Alternar(int id)
        {
            var r = _context.Resenas.Find(id);
            if (r != null)
            {
                r.Visible = !r.Visible;
                _context.SaveChanges();
                TempData["Mensaje"] = r.Visible ? "Reseña visible en la tienda." : "Reseña oculta.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var r = _context.Resenas.Find(id);
            if (r != null)
            {
                _context.Resenas.Remove(r);
                _context.SaveChanges();
                TempData["Mensaje"] = "Reseña eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
