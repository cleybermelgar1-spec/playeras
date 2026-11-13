using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PlayerasJyC.Controllers
{
    // Controlador base: lee la sesión y protege las secciones según el rol.
    [AutoValidateAntiforgeryToken]
    public abstract class BaseController : Controller
    {
        protected int? UsuarioId => HttpContext.Session.GetInt32("UsuarioId");
        protected string? Rol => HttpContext.Session.GetString("Rol");

        // null = sección pública, "Cliente" = solo clientes, "Admin" = solo administradores
        protected virtual string? RolRequerido => null;

        // true = esta acción pide iniciar sesión (cualquier rol)
        protected virtual bool RequiereSesion(string accion) => false;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var accionActual = context.ActionDescriptor.RouteValues["action"] ?? "";

            if (RolRequerido == null && UsuarioId == null && RequiereSesion(accionActual))
            {
                TempData["Error"] = "Inicia sesión para continuar.";
                context.Result = RedirectToAction("Ingresar", "Cuenta", new { regresarA = RutaDeRegreso() });
            }
            else if (RolRequerido != null)
            {
                if (UsuarioId == null)
                {
                    TempData["Error"] = "Inicia sesión para continuar.";
                    context.Result = RedirectToAction("Ingresar", "Cuenta", new { regresarA = RutaDeRegreso() });
                }
                else if (Rol != RolRequerido)
                {
                    TempData["Error"] = RolRequerido == "Admin"
                        ? "Esta sección es solo para administradores."
                        : "Esta sección es solo para clientes.";
                    context.Result = RedirectToAction("Index", "Home");
                }
            }

            base.OnActionExecuting(context);
        }

        private string? RutaDeRegreso()
        {
            return Request.Method == "GET" ? Request.Path + Request.QueryString : null;
        }

        protected IActionResult RedirigirLocal(string? url, string accion, string controlador)
        {
            if (!string.IsNullOrEmpty(url) && Url.IsLocalUrl(url))
            {
                return LocalRedirect(url);
            }
            return RedirectToAction(accion, controlador);
        }

        protected void GuardarContadorCarrito(int cantidad)
        {
            HttpContext.Session.SetInt32("CarritoCount", cantidad);
        }
    }
}