using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Catálogo: Tallas (solo administradores)
    public class AdminTallasController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminTallasController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            var lista = _context.Tallas.AsNoTracking().OrderBy(x => x.Orden).ToList();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View("Formulario", new Talla());
        }

        [HttpPost]
        public IActionResult Crear([Bind("TallaId,Nombre,Orden")] Talla modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Tallas.Add(modelo);
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
            var m = _context.Tallas.Find(id);
            if (m == null) return NotFound();
            return View("Formulario", m);
        }

        [HttpPost]
        public IActionResult Editar(int id, [Bind("TallaId,Nombre,Orden")] Talla modelo)
        {
            if (id != modelo.TallaId) return NotFound();
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
            var m = _context.Tallas.Find(id);
            if (m != null)
            {
                try
                {
                    _context.Tallas.Remove(m);
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
