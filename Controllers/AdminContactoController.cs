using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;

namespace PlayerasJyC.Controllers
{
    // Solicitudes de cotización enviadas desde "Contáctanos"
    public class AdminContactoController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminContactoController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            var lista = _context.SolicitudesContacto
                .AsNoTracking()
                .OrderBy(s => s.Atendida)
                .ThenByDescending(s => s.FechaSolicitud)
                .ToList();
            return View(lista);
        }

        [HttpPost]
        public IActionResult Alternar(int id)
        {
            var s = _context.SolicitudesContacto.Find(id);
            if (s != null)
            {
                s.Atendida = !s.Atendida;
                _context.SaveChanges();
                TempData["Mensaje"] = s.Atendida ? "Solicitud marcada como atendida." : "Solicitud marcada como pendiente.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var s = _context.SolicitudesContacto.Find(id);
            if (s != null)
            {
                _context.SolicitudesContacto.Remove(s);
                _context.SaveChanges();
                TempData["Mensaje"] = "Solicitud eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
