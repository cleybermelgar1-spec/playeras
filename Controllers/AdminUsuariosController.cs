using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Administración de usuarios: crear, editar y eliminar (solo administradores)
    public class AdminUsuariosController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminUsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index()
        {
            var lista = _context.Usuarios.AsNoTracking().OrderBy(u => u.Rol).ThenBy(u => u.Nombre).ToList();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View("Formulario", new Usuario());
        }

        [HttpPost]
        public IActionResult Crear([Bind("UsuarioId,Nombre,Email,Telefono,Rol,Activo,FechaRegistro")] Usuario modelo, string? password)
        {
            modelo.Email = (modelo.Email ?? "").Trim().ToLowerInvariant();
            Validar(modelo, password, true);

            if (ModelState.IsValid)
            {
                modelo.PasswordHash = Seguridad.Hash(password!);
                modelo.FechaRegistro = DateTime.Now;
                _context.Usuarios.Add(modelo);
                _context.SaveChanges();
                TempData["Mensaje"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View("Formulario", modelo);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var u = _context.Usuarios.Find(id);
            if (u == null) return NotFound();
            return View("Formulario", u);
        }

        [HttpPost]
        public IActionResult Editar(int id, [Bind("UsuarioId,Nombre,Email,Telefono,Rol,Activo,FechaRegistro")] Usuario modelo, string? password)
        {
            if (id != modelo.UsuarioId) return NotFound();
            modelo.Email = (modelo.Email ?? "").Trim().ToLowerInvariant();

            // Un administrador no puede quitarse su propio acceso
            if (id == UsuarioId)
            {
                modelo.Rol = "Admin";
                modelo.Activo = true;
            }

            Validar(modelo, password, false);

            if (ModelState.IsValid)
            {
                var actual = _context.Usuarios.AsNoTracking().Where(u => u.UsuarioId == id).Select(u => u.PasswordHash).FirstOrDefault();
                modelo.PasswordHash = string.IsNullOrEmpty(password) ? (actual ?? "") : Seguridad.Hash(password);
                _context.Update(modelo);
                _context.SaveChanges();
                TempData["Mensaje"] = "Usuario actualizado.";
                return RedirectToAction(nameof(Index));
            }
            return View("Formulario", modelo);
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (id == UsuarioId)
            {
                TempData["Error"] = "No puedes eliminar tu propia cuenta.";
                return RedirectToAction(nameof(Index));
            }

            var u = _context.Usuarios.Find(id);
            if (u != null)
            {
                try
                {
                    _context.Usuarios.Remove(u);
                    _context.SaveChanges();
                    TempData["Mensaje"] = "Usuario eliminado.";
                }
                catch (DbUpdateException)
                {
                    _context.ChangeTracker.Clear();
                    TempData["Error"] = "Ese usuario tiene pedidos registrados y no se puede borrar. Desactívalo desde Editar.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private void Validar(Usuario m, string? password, bool esNuevo)
        {
            if (m.Rol != "Admin" && m.Rol != "Cliente")
                ModelState.AddModelError("", "Elige un rol válido.");

            if (m.Email.Length == 0 || m.Email.Length > 100 || !new EmailAddressAttribute().IsValid(m.Email))
                ModelState.AddModelError("", "Escribe un correo válido.");
            else if (_context.Usuarios.Any(u => u.Email == m.Email && u.UsuarioId != m.UsuarioId))
                ModelState.AddModelError("", "Ese correo ya está registrado.");

            if (esNuevo && (string.IsNullOrEmpty(password) || password.Length < 6))
                ModelState.AddModelError("", "La contraseña es obligatoria (mínimo 6 caracteres).");
            else if (!esNuevo && !string.IsNullOrEmpty(password) && password.Length < 6)
                ModelState.AddModelError("", "La nueva contraseña debe tener mínimo 6 caracteres.");
        }
    }
}
