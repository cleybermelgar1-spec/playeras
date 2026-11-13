using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Catálogo: Categorías (solo administradores)
    public class AdminCategoriasController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminCategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            var lista = _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToList();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View("Formulario", new Categoria());
        }

        [HttpPost]
        public IActionResult Crear([Bind("CategoriaId,Nombre,Descripcion,Activo")] Categoria modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Categorias.Add(modelo);
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
            var m = _context.Categorias.Find(id);
            if (m == null) return NotFound();
            return View("Formulario", m);
        }

        [HttpPost]
        public IActionResult Editar(int id, [Bind("CategoriaId,Nombre,Descripcion,Activo")] Categoria modelo)
        {
            if (id != modelo.CategoriaId) return NotFound();
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
            var m = _context.Categorias.Find(id);
            if (m != null)
            {
                try
                {
                    _context.Categorias.Remove(m);
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
