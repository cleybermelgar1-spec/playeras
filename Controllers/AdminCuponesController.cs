using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Catálogo: Cupones de descuento (solo administradores)
    public class AdminCuponesController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminCuponesController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            var lista = _context.Cupones.AsNoTracking().OrderBy(x => x.Codigo).ToList();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View("Formulario", new Cupon());
        }

        [HttpPost]
        public IActionResult Crear([Bind("CuponId,Codigo,Descripcion,Tipo,Valor,MinimoCompra,FechaInicio,FechaFin,UsosMaximos,UsosActuales,Activo")] Cupon modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Cupones.Add(modelo);
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
            var m = _context.Cupones.Find(id);
            if (m == null) return NotFound();
            return View("Formulario", m);
        }

        [HttpPost]
        public IActionResult Editar(int id, [Bind("CuponId,Codigo,Descripcion,Tipo,Valor,MinimoCompra,FechaInicio,FechaFin,UsosMaximos,UsosActuales,Activo")] Cupon modelo)
        {
            if (id != modelo.CuponId) return NotFound();
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
            var m = _context.Cupones.Find(id);
            if (m != null)
            {
                try
                {
                    _context.Cupones.Remove(m);
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
