using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Catálogo: Colecciones (solo administradores)
    public class AdminColeccionesController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminColeccionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            var lista = _context.Colecciones.AsNoTracking().OrderBy(x => x.Nombre).ToList();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View("Formulario", new Coleccion());
        }

        [HttpPost]
        public IActionResult Crear([Bind("ColeccionId,Nombre,Descripcion,Activo")] Coleccion modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Colecciones.Add(modelo);
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
            var m = _context.Colecciones.Find(id);
            if (m == null) return NotFound();
            return View("Formulario", m);
        }

        [HttpPost]
        public IActionResult Editar(int id, [Bind("ColeccionId,Nombre,Descripcion,Activo")] Coleccion modelo)
        {
            if (id != modelo.ColeccionId) return NotFound();
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
            var m = _context.Colecciones.Find(id);
            if (m != null)
            {
                try
                {
                    _context.Colecciones.Remove(m);
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
