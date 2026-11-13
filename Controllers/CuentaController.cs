using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Registro e inicio de sesión (clientes y administradores)
    public class CuentaController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CuentaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Registro()
        {
            if (UsuarioId != null) return RedirectToAction("Index", "MiCuenta");
            return View();
        }

        [HttpPost]
        public IActionResult Registro(string nombre, string email, string? telefono, string password, string confirmar)
        {
            nombre = (nombre ?? "").Trim();
            email = (email ?? "").Trim().ToLowerInvariant();
            telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();

            string? error = null;
            if (nombre.Length == 0 || nombre.Length > 100) error = "Escribe tu nombre (máximo 100 caracteres).";
            else if (email.Length > 100 || !new EmailAddressAttribute().IsValid(email)) error = "Escribe un correo electrónico válido.";
            else if (telefono != null && telefono.Length > 20) error = "El teléfono es demasiado largo (máximo 20 caracteres).";
            else if (string.IsNullOrEmpty(password) || password.Length < 6) error = "La contraseña debe tener mínimo 6 caracteres.";
            else if (password != confirmar) error = "Las contraseñas no coinciden.";
            else if (_context.Usuarios.Any(u => u.Email == email)) error = "Ese correo ya está registrado. Inicia sesión.";

            if (error != null)
            {
                ViewBag.Error = error;
                ViewBag.Nombre = nombre;
                ViewBag.Email = email;
                ViewBag.Telefono = telefono;
                return View();
            }

            var usuario = new Usuario
            {
                Nombre = nombre,
                Email = email,
                Telefono = telefono,
                PasswordHash = Seguridad.Hash(password),
                Rol = "Cliente",
                Activo = true,
                FechaRegistro = DateTime.Now
            };
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            IniciarSesion(usuario);
            TempData["Mensaje"] = "¡Cuenta creada! Bienvenido, " + usuario.Nombre + ".";
            return RedirectToAction("Index", "Productos");
        }

        [HttpGet]
        public IActionResult Ingresar(string? regresarA)
        {
            if (UsuarioId != null) return RedirectToAction("Index", "Home");
            ViewBag.RegresarA = regresarA;
            return View();
        }

        [HttpPost]
        public IActionResult Ingresar(string email, string password, string? regresarA)
        {
            email = (email ?? "").Trim().ToLowerInvariant();

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Activo);
            if (usuario == null || !Seguridad.Verificar(password ?? "", usuario.PasswordHash))
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                ViewBag.RegresarA = regresarA;
                return View();
            }

            IniciarSesion(usuario);
            TempData["Mensaje"] = "¡Hola de nuevo, " + usuario.Nombre + "!";

            if (usuario.Rol == "Admin") return RedirigirLocal(regresarA, "Index", "Admin");
            return RedirigirLocal(regresarA, "Index", "Productos");
        }

        [HttpPost]
        public IActionResult Salir()
        {
            HttpContext.Session.Clear();
            TempData["Mensaje"] = "Cerraste sesión correctamente.";
            return RedirectToAction("Index", "Home");
        }

        private void IniciarSesion(Usuario u)
        {
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UsuarioId", u.UsuarioId);
            HttpContext.Session.SetString("Nombre", u.Nombre);
            HttpContext.Session.SetString("Rol", u.Rol);

            int n = _context.CarritoItems.Where(c => c.UsuarioId == u.UsuarioId).Sum(c => (int?)c.Cantidad) ?? 0;
            GuardarContadorCarrito(n);
        }
    }
}
