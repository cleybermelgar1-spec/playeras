using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Catálogo: Métodos de envío (solo administradores)
    public class AdminEnviosController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminEnviosController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            var lista = _context.MetodosEnvio.AsNoTracking().OrderBy(x => x.Costo).ToList();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View("Formulario", new MetodoEnvio());
        }

        [HttpPost]
        public IActionResult Crear([Bind("MetodoEnvioId,Nombre,Costo,DiasEstimados,Activo")] MetodoEnvio modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.MetodosEnvio.Add(modelo);
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
            var m = _context.MetodosEnvio.Find(id);
            if (m == null) return NotFound();
            return View("Formulario", m);
        }

        [HttpPost]
        public IActionResult Editar(int id, [Bind("MetodoEnvioId,Nombre,Costo,DiasEstimados,Activo")] MetodoEnvio modelo)
        {
            if (id != modelo.MetodoEnvioId) return NotFound();
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
            var m = _context.MetodosEnvio.Find(id);
            if (m != null)
            {
                try
                {
                    _context.MetodosEnvio.Remove(m);
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
