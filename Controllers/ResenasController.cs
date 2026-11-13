using Microsoft.AspNetCore.Mvc;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Reseñas de productos (solo clientes con sesión)
    public class ResenasController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ResenasController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Cliente";

        [HttpPost]
        public IActionResult Guardar(int productoId, int calificacion, string? comentario)
        {
            int uid = UsuarioId!.Value;

            if (!_context.Productos.Any(p => p.ProductoId == productoId)) return NotFound();
            if (calificacion < 1 || calificacion > 5)
            {
                TempData["Error"] = "Elige una calificación de 1 a 5 estrellas.";
                return RedirectToAction("Detalle", "Productos", new { id = productoId });
            }

            comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
            if (comentario != null && comentario.Length > 500) comentario = comentario.Substring(0, 500);

            var resena = _context.Resenas.FirstOrDefault(r => r.ProductoId == productoId && r.UsuarioId == uid);
            if (resena == null)
            {
                _context.Resenas.Add(new Resena
                {
                    ProductoId = productoId,
                    UsuarioId = uid,
                    Calificacion = calificacion,
                    Comentario = comentario,
                    Fecha = DateTime.Now,
                    Visible = true
                });
            }
            else
            {
                resena.Calificacion = calificacion;
                resena.Comentario = comentario;
                resena.Fecha = DateTime.Now;
            }

            _context.SaveChanges();
            TempData["Mensaje"] = "¡Gracias por tu reseña!";
            return RedirectToAction("Detalle", "Productos", new { id = productoId });
        }
    }
}
