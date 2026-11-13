using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Catálogo de productos: alta, edición, baja, tallas/colores (inventario) e imágenes
    public class AdminProductosController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminProductosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        protected override string? RolRequerido => "Admin";

        public IActionResult Index(string? q)
        {
            var query = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Variantes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var texto = q.Trim();
                query = query.Where(p => p.Nombre.Contains(texto));
            }

            ViewBag.Q = q;
            return View(query.OrderBy(p => p.Nombre).ToList());
        }

        private void CargarListas(Producto? p = null)
        {
            ViewBag.Categorias = new SelectList(_context.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToList(), "CategoriaId", "Nombre", p?.CategoriaId);
            ViewBag.Colecciones = new SelectList(_context.Colecciones.AsNoTracking().OrderBy(c => c.Nombre).ToList(), "ColeccionId", "Nombre", p?.ColeccionId);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            var nuevo = new Producto();
            CargarListas(nuevo);
            return View("Formulario", nuevo);
        }

        [HttpPost]
        public IActionResult Crear([Bind("ProductoId,CategoriaId,ColeccionId,Nombre,Descripcion,Material,PrecioOriginal,EnOferta,PrecioOferta,ImagenUrl,Activo,FechaAlta")] Producto modelo, IFormFile? archivo)
        {
            ValidarProducto(modelo);

            if (ModelState.IsValid)
            {
                var ruta = GuardarImagen(archivo, out string? errorImg);
                if (errorImg != null)
                {
                    ModelState.AddModelError("", errorImg);
                }
                else
                {
                    if (ruta != null) modelo.ImagenUrl = ruta;
                    modelo.FechaAlta = DateTime.Now;
                    _context.Productos.Add(modelo);
                    _context.SaveChanges();
                    TempData["Mensaje"] = "Producto creado. Ahora agrega sus tallas, colores y existencias.";
                    return RedirectToAction(nameof(Variantes), new { id = modelo.ProductoId });
                }
            }

            CargarListas(modelo);
            return View("Formulario", modelo);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var p = _context.Productos.Find(id);
            if (p == null) return NotFound();
            CargarListas(p);
            return View("Formulario", p);
        }

        [HttpPost]
        public IActionResult Editar(int id, [Bind("ProductoId,CategoriaId,ColeccionId,Nombre,Descripcion,Material,PrecioOriginal,EnOferta,PrecioOferta,ImagenUrl,Activo,FechaAlta")] Producto modelo, IFormFile? archivo)
        {
            if (id != modelo.ProductoId) return NotFound();
            ValidarProducto(modelo);

            if (ModelState.IsValid)
            {
                var ruta = GuardarImagen(archivo, out string? errorImg);
                if (errorImg != null)
                {
                    ModelState.AddModelError("", errorImg);
                }
                else
                {
                    if (ruta != null) modelo.ImagenUrl = ruta;
                    _context.Update(modelo);
                    _context.SaveChanges();
                    TempData["Mensaje"] = "Producto actualizado.";
                    return RedirectToAction(nameof(Index));
                }
            }

            CargarListas(modelo);
            return View("Formulario", modelo);
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var p = _context.Productos.Find(id);
            if (p != null)
            {
                try
                {
                    _context.Productos.Remove(p);
                    _context.SaveChanges();
                    TempData["Mensaje"] = "Producto eliminado.";
                }
                catch (DbUpdateException)
                {
                    _context.ChangeTracker.Clear();
                    var again = _context.Productos.Find(id);
                    if (again != null)
                    {
                        again.Activo = false;
                        _context.SaveChanges();
                    }
                    TempData["Error"] = "Ese producto ya tiene ventas, por eso no se puede borrar. Se desactivó para que no aparezca en la tienda.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private void ValidarProducto(Producto m)
        {
            if (m.EnOferta)
            {
                if (!m.PrecioOferta.HasValue || m.PrecioOferta.Value <= 0)
                    ModelState.AddModelError("", "Si el producto está en oferta, escribe el precio de oferta.");
                else if (m.PrecioOferta.Value >= m.PrecioOriginal)
                    ModelState.AddModelError("", "El precio de oferta debe ser menor al precio original.");
            }
            else
            {
                m.PrecioOferta = null;
            }
        }

        // ---------- INVENTARIO: tallas y colores ----------
        public IActionResult Variantes(int id)
        {
            var p = _context.Productos.AsNoTracking().FirstOrDefault(x => x.ProductoId == id);
            if (p == null) return NotFound();

            ViewBag.Producto = p;
            ViewBag.Tallas = new SelectList(_context.Tallas.AsNoTracking().OrderBy(t => t.Orden).ToList(), "TallaId", "Nombre");
            ViewBag.Colores = new SelectList(_context.Colores.AsNoTracking().OrderBy(c => c.Nombre).ToList(), "ColorId", "Nombre");

            var lista = _context.Variantes
                .AsNoTracking()
                .Include(v => v.Talla)
                .Include(v => v.Color)
                .Where(v => v.ProductoId == id)
                .OrderBy(v => v.Talla.Orden)
                .ThenBy(v => v.Color.Nombre)
                .ToList();
            return View(lista);
        }

        [HttpPost]
        public IActionResult AgregarVariante(int productoId, int tallaId, int colorId, int stock, string? sku)
        {
            if (stock < 0)
            {
                TempData["Error"] = "El stock no puede ser negativo.";
                return RedirectToAction(nameof(Variantes), new { id = productoId });
            }
            if (_context.Variantes.Any(v => v.ProductoId == productoId && v.TallaId == tallaId && v.ColorId == colorId))
            {
                TempData["Error"] = "Esa combinación de talla y color ya existe. Edita su stock en la tabla.";
                return RedirectToAction(nameof(Variantes), new { id = productoId });
            }

            sku = string.IsNullOrWhiteSpace(sku) ? null : sku.Trim();
            if (sku != null && sku.Length > 40) sku = sku.Substring(0, 40);

            _context.Variantes.Add(new Variante { ProductoId = productoId, TallaId = tallaId, ColorId = colorId, Stock = stock, Sku = sku });
            _context.SaveChanges();
            TempData["Mensaje"] = "Talla y color agregados.";
            return RedirectToAction(nameof(Variantes), new { id = productoId });
        }

        [HttpPost]
        public IActionResult ActualizarStock(int id, int stock)
        {
            var v = _context.Variantes.Find(id);
            if (v == null) return NotFound();
            if (stock < 0)
            {
                TempData["Error"] = "El stock no puede ser negativo.";
            }
            else
            {
                v.Stock = stock;
                _context.SaveChanges();
                TempData["Mensaje"] = "Stock actualizado.";
            }
            return RedirectToAction(nameof(Variantes), new { id = v.ProductoId });
        }

        [HttpPost]
        public IActionResult EliminarVariante(int id)
        {
            var v = _context.Variantes.Find(id);
            if (v == null) return NotFound();
            int productoId = v.ProductoId;
            try
            {
                _context.Variantes.Remove(v);
                _context.SaveChanges();
                TempData["Mensaje"] = "Variante eliminada.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Esa variante ya tiene ventas y no se puede borrar. Pon su stock en 0.";
            }
            return RedirectToAction(nameof(Variantes), new { id = productoId });
        }

        // ---------- IMÁGENES ----------
        public IActionResult Imagenes(int id)
        {
            var p = _context.Productos.AsNoTracking().FirstOrDefault(x => x.ProductoId == id);
            if (p == null) return NotFound();
            ViewBag.Producto = p;
            var lista = _context.ImagenesProducto.AsNoTracking().Where(i => i.ProductoId == id).OrderBy(i => i.Orden).ToList();
            return View(lista);
        }

        [HttpPost]
        public IActionResult AgregarImagen(int productoId, IFormFile? archivo, string? url)
        {
            var producto = _context.Productos.Find(productoId);
            if (producto == null) return NotFound();

            string? ruta = GuardarImagen(archivo, out string? errorImg);
            if (errorImg != null)
            {
                TempData["Error"] = errorImg;
                return RedirectToAction(nameof(Imagenes), new { id = productoId });
            }

            if (ruta == null && !string.IsNullOrWhiteSpace(url)) ruta = url.Trim();
            if (ruta == null || ruta.Length > 300)
            {
                TempData["Error"] = "Sube un archivo o escribe una dirección de imagen válida (máximo 300 caracteres).";
                return RedirectToAction(nameof(Imagenes), new { id = productoId });
            }

            int orden = (_context.ImagenesProducto.Where(i => i.ProductoId == productoId).Max(i => (int?)i.Orden) ?? 0) + 1;
            _context.ImagenesProducto.Add(new ImagenProducto { ProductoId = productoId, Url = ruta, Orden = orden });
            if (string.IsNullOrWhiteSpace(producto.ImagenUrl)) producto.ImagenUrl = ruta;
            _context.SaveChanges();
            TempData["Mensaje"] = "Imagen agregada.";
            return RedirectToAction(nameof(Imagenes), new { id = productoId });
        }

        [HttpPost]
        public IActionResult UsarComoPrincipal(int id)
        {
            var img = _context.ImagenesProducto.Find(id);
            if (img == null) return NotFound();
            var producto = _context.Productos.Find(img.ProductoId);
            if (producto != null)
            {
                producto.ImagenUrl = img.Url;
                _context.SaveChanges();
                TempData["Mensaje"] = "Imagen principal actualizada.";
            }
            return RedirectToAction(nameof(Imagenes), new { id = img.ProductoId });
        }

        [HttpPost]
        public IActionResult EliminarImagen(int id)
        {
            var img = _context.ImagenesProducto.Find(id);
            if (img == null) return NotFound();
            int productoId = img.ProductoId;
            _context.ImagenesProducto.Remove(img);
            _context.SaveChanges();
            TempData["Mensaje"] = "Imagen eliminada.";
            return RedirectToAction(nameof(Imagenes), new { id = productoId });
        }

        // Guarda el archivo subido en wwwroot/uploads/productos y devuelve su ruta pública
        private string? GuardarImagen(IFormFile? archivo, out string? error)
        {
            error = null;
            if (archivo == null || archivo.Length == 0) return null;

            var permitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!permitidas.Contains(ext))
            {
                error = "Formato no permitido. Usa JPG, PNG, WEBP o GIF.";
                return null;
            }
            if (archivo.Length > 5 * 1024 * 1024)
            {
                error = "La imagen es demasiado grande (máximo 5 MB).";
                return null;
            }

            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "productos");
            Directory.CreateDirectory(carpeta);
            var nombre = Guid.NewGuid().ToString("N") + ext;
            using (var fs = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create))
            {
                archivo.CopyTo(fs);
            }
            return "/uploads/productos/" + nombre;
        }
    }
}
