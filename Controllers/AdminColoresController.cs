using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Catálogo: Colores (solo administradores)
    public class AdminColoresController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminColoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            var lista = _context.Colores.AsNoTracking().OrderBy(x => x.Nombre).ToList();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View("Formulario", new Color());
        }

        [HttpPost]
        public IActionResult Crear([Bind("ColorId,Nombre,CodigoHex")] Color modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Colores.Add(modelo);
                    _context.SaveChanges();
                    TempData["Mensaje"] = "Registro creado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "No se pudo guardar. Verifica que no esté repetido.");
                }
            }
            return View("Formulario", modelo);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var m = _context.Colores.Find(id);
            if (m == null) return NotFound();
            return View("Formulario", m);
        }

        [HttpPost]
        public IActionResult Editar(int id, [Bind("ColorId,Nombre,CodigoHex")] Color modelo)
        {
            if (id != modelo.ColorId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(modelo);
                    _context.SaveChanges();
                    TempData["Mensaje"] = "Registro actualizado.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "No se pudo guardar. Verifica que no esté repetido.");
                }
            }
            return View("Formulario", modelo);
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var m = _context.Colores.Find(id);
            if (m != null)
            {
                try
                {
                    _context.Colores.Remove(m);
                    _context.SaveChanges();
                    TempData["Mensaje"] = "Registro eliminado.";
                }
                catch (DbUpdateException)
                {
                    _context.ChangeTracker.Clear();
                    TempData["Error"] = "No se puede eliminar porque ya se está usando en otros registros.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
