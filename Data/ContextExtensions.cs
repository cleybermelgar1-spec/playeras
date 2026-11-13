using Microsoft.EntityFrameworkCore;
using PlayerasJyC.Models;

namespace PlayerasJyC.Data
{
    public static class ContextExtensions
    {
        // Carga un pedido con todo lo necesario para mostrar el ticket o el detalle
        public static Pedido? PedidoCompleto(this ApplicationDbContext ctx, int id)
        {
            return ctx.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.MetodoEnvio)
                .Include(p => p.Cupon)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .Include(p => p.Pagos)
                .Include(p => p.Historial)
                .AsSplitQuery()
                .FirstOrDefault(p => p.PedidoId == id);
        }
    }
}
