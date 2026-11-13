using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Página de inicio: ofertas y novedades desde SQL Server
        public IActionResult Index()
        {
            ViewBag.Novedades = _context.Productos
                .AsNoTracking()
                .Include(p => p.Variantes)
                .Where(p => p.Activo && !p.EnOferta)
                .OrderByDescending(p => p.FechaAlta)
                .Take(6)
                .ToList();

            var ofertas = _context.Productos
                .AsNoTracking()
                .Include(p => p.Variantes)
                .Where(p => p.Activo && p.EnOferta && p.PrecioOferta != null)
                .OrderBy(p => p.Nombre)
                .ToList();

            return View(ofertas);
        }

        [HttpGet]
        public IActionResult Contacto()
        {
            return View();
        }

        // Guarda la solicitud de cotización en la base de datos
        [HttpPost]
        public IActionResult Contacto(string nombre, string email, string playera, string talla, string? detalles)
        {
            nombre = (nombre ?? "").Trim();
            email = (email ?? "").Trim();
            playera = (playera ?? "").Trim();
            talla = (talla ?? "").Trim();

            if (nombre.Length == 0 || email.Length == 0 || playera.Length == 0 || talla.Length == 0)
            {
                ViewBag.Error = "Completa todos los campos obligatorios.";
                return View();
            }

            var solicitud = new SolicitudContacto
            {
                NombreCompleto = nombre.Length > 100 ? nombre.Substring(0, 100) : nombre,
                Email = email.Length > 100 ? email.Substring(0, 100) : email,
                PlayeraSolicitada = playera.Length > 100 ? playera.Substring(0, 100) : playera,
                Talla = talla.Length > 10 ? talla.Substring(0, 10) : talla,
                Detalles = string.IsNullOrWhiteSpace(detalles) ? null : (detalles.Length > 500 ? detalles.Substring(0, 500) : detalles),
                FechaSolicitud = DateTime.Now
            };

            _context.SolicitudesContacto.Add(solicitud);
            _context.SaveChanges();

            ViewBag.Mensaje = "¡Gracias " + nombre + "! Tu solicitud para '" + playera + "' ha sido enviada con éxito.";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
