using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Data;
using PlayerasJyC.Models;

namespace PlayerasJyC.Controllers
{
    // Catálogo público: lista con filtros y detalle del producto
    public class ProductosController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }
        protected override bool RequiereSesion(string accion) => true;

        public IActionResult Index(int? categoriaId, int? coleccionId, string? q, string? orden)
        {
            var query = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Variantes)
                .Where(p => p.Activo);

            if (categoriaId.HasValue) query = query.Where(p => p.CategoriaId == categoriaId.Value);
            if (coleccionId.HasValue) query = query.Where(p => p.ColeccionId == coleccionId.Value);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var texto = q.Trim();
                query = query.Where(p => p.Nombre.Contains(texto) || (p.Descripcion != null && p.Descripcion.Contains(texto)));
            }

            switch (orden)
            {
                case "precio_asc":
                    query = query.OrderBy(p => p.EnOferta && p.PrecioOferta != null ? p.PrecioOferta.Value : p.PrecioOriginal);
                    break;
                case "precio_desc":
                    query = query.OrderByDescending(p => p.EnOferta && p.PrecioOferta != null ? p.PrecioOferta.Value : p.PrecioOriginal);
                    break;
                case "nuevo":
                    query = query.OrderByDescending(p => p.FechaAlta);
                    break;
                default:
                    query = query.OrderBy(p => p.Nombre);
                    break;
            }

            ViewBag.Categorias = _context.Categorias.AsNoTracking().Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();
            ViewBag.Colecciones = _context.Colecciones.AsNoTracking().Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();
            ViewBag.CategoriaId = categoriaId;
            ViewBag.ColeccionId = coleccionId;
            ViewBag.Q = q;
            ViewBag.Orden = orden;

            return View(query.ToList());
        }

        public IActionResult Detalle(int id)
        {
            var producto = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Coleccion)
                .Include(p => p.Imagenes)
                .Include(p => p.Variantes).ThenInclude(v => v.Talla)
                .Include(p => p.Variantes).ThenInclude(v => v.Color)
                .AsSplitQuery()
                .FirstOrDefault(p => p.ProductoId == id && p.Activo);

            if (producto == null) return NotFound();

            var resenas = _context.Resenas
                .AsNoTracking()
                .Include(r => r.Usuario)
                .Where(r => r.ProductoId == id && r.Visible)
                .OrderByDescending(r => r.Fecha)
                .ToList();

            var vm = new ProductoDetalleVM
            {
                Producto = producto,
                Variantes = producto.Variantes
                    .Where(v => v.Stock > 0)
                    .OrderBy(v => v.Talla.Orden)
                    .ThenBy(v => v.Color.Nombre)
                    .ToList(),
                Resenas = resenas,
                Promedio = resenas.Count == 0 ? 0 : resenas.Average(r => r.Calificacion),
                EsCliente = UsuarioId != null && Rol == "Cliente"
            };

            if (vm.EsCliente)
            {
                int uid = UsuarioId!.Value;
                vm.MiResena = _context.Resenas.AsNoTracking().FirstOrDefault(r => r.ProductoId == id && r.UsuarioId == uid);
            }

            return View(vm);
        }
    }
}
