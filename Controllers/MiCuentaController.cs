using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Perfil del cliente y sus direcciones de envío
    public class MiCuentaController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public MiCuentaController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Cliente";

        public IActionResult Index()
        {
            int uid = UsuarioId!.Value;
            var usuario = _context.Usuarios.AsNoTracking().FirstOrDefault(u => u.UsuarioId == uid);
            if (usuario == null) return RedirectToAction("Index", "Home");

            var vm = new MiCuentaVM
            {
                Usuario = usuario,
                Direcciones = _context.Direcciones
                    .AsNoTracking()
                    .Where(d => d.UsuarioId == uid)
                    .OrderByDescending(d => d.Predeterminada)
                    .ThenBy(d => d.Alias)
                    .ToList()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult GuardarPerfil(string nombre, string? telefono)
        {
            int uid = UsuarioId!.Value;
            var usuario = _context.Usuarios.Find(uid);
            if (usuario == null) return RedirectToAction("Index", "Home");

            nombre = (nombre ?? "").Trim();
            telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
            if (nombre.Length == 0 || nombre.Length > 100 || (telefono != null && telefono.Length > 20))
            {
                TempData["Error"] = "Revisa tus datos: el nombre es obligatorio y el teléfono máximo 20 caracteres.";
                return RedirectToAction(nameof(Index));
            }

            usuario.Nombre = nombre;
            usuario.Telefono = telefono;
            _context.SaveChanges();
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
            TempData["Mensaje"] = "Tus datos se actualizaron.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CambiarPassword(string actual, string nueva, string confirmar)
        {
            int uid = UsuarioId!.Value;
            var usuario = _context.Usuarios.Find(uid);
            if (usuario == null) return RedirectToAction("Index", "Home");

            if (!Seguridad.Verificar(actual ?? "", usuario.PasswordHash))
                TempData["Error"] = "La contraseña actual no es correcta.";
            else if (string.IsNullOrEmpty(nueva) || nueva.Length < 6)
                TempData["Error"] = "La nueva contraseña debe tener mínimo 6 caracteres.";
            else if (nueva != confirmar)
                TempData["Error"] = "Las contraseñas nuevas no coinciden.";
            else
            {
                usuario.PasswordHash = Seguridad.Hash(nueva);
                _context.SaveChanges();
                TempData["Mensaje"] = "Contraseña actualizada.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AgregarDireccion(string alias, string nombreDestinatario, string calle, string? colonia,
            string ciudad, string estado, string codigoPostal, string? telefono, bool predeterminada)
        {
            int uid = UsuarioId!.Value;

            alias = (alias ?? "").Trim();
            nombreDestinatario = (nombreDestinatario ?? "").Trim();
            calle = (calle ?? "").Trim();
            colonia = string.IsNullOrWhiteSpace(colonia) ? null : colonia.Trim();
            ciudad = (ciudad ?? "").Trim();
            estado = (estado ?? "").Trim();
            codigoPostal = (codigoPostal ?? "").Trim();
            telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();

            if (alias.Length == 0) alias = "Casa";
            if (nombreDestinatario.Length < 3 || calle.Length < 3 || ciudad.Length < 2 || estado.Length < 2
                || !System.Text.RegularExpressions.Regex.IsMatch(codigoPostal, @"^\d{5}$"))
            {
                TempData["Error"] = "Revisa la dirección: todos los campos son obligatorios y el código postal debe tener 5 dígitos.";
                return RedirectToAction(nameof(Index));
            }

            bool primera = !_context.Direcciones.Any(d => d.UsuarioId == uid);
            if (predeterminada || primera)
            {
                var anteriores = _context.Direcciones.Where(d => d.UsuarioId == uid && d.Predeterminada).ToList();
                foreach (var d in anteriores) d.Predeterminada = false;
                predeterminada = true;
            }

            _context.Direcciones.Add(new Direccion
            {
                UsuarioId = uid,
                Alias = alias.Length > 50 ? alias.Substring(0, 50) : alias,
                NombreDestinatario = nombreDestinatario.Length > 100 ? nombreDestinatario.Substring(0, 100) : nombreDestinatario,
                Calle = calle.Length > 150 ? calle.Substring(0, 150) : calle,
                Colonia = colonia != null && colonia.Length > 100 ? colonia.Substring(0, 100) : colonia,
                Ciudad = ciudad.Length > 100 ? ciudad.Substring(0, 100) : ciudad,
                Estado = estado.Length > 100 ? estado.Substring(0, 100) : estado,
                CodigoPostal = codigoPostal,
                Telefono = telefono != null && telefono.Length > 20 ? telefono.Substring(0, 20) : telefono,
                Predeterminada = predeterminada
            });
            _context.SaveChanges();
            TempData["Mensaje"] = "Dirección guardada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult EliminarDireccion(int id)
        {
            int uid = UsuarioId!.Value;
            var d = _context.Direcciones.FirstOrDefault(x => x.DireccionId == id && x.UsuarioId == uid);
            if (d != null)
            {
                _context.Direcciones.Remove(d);
                _context.SaveChanges();
                TempData["Mensaje"] = "Dirección eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Predeterminada(int id)
        {
            int uid = UsuarioId!.Value;
            var todas = _context.Direcciones.Where(d => d.UsuarioId == uid).ToList();
            if (todas.Any(d => d.DireccionId == id))
            {
                foreach (var d in todas) d.Predeterminada = d.DireccionId == id;
                _context.SaveChanges();
                TempData["Mensaje"] = "Dirección predeterminada actualizada.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
