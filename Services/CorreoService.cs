using System.Net;
using System.Net.Mail;
using System.Text;
using PlayerasJyC.Models;

namespace PlayerasJyC.Services
{
    // Envía el ticket por correo usando la cuenta configurada en appsettings.json ("Correo").
    // Si no hay configuración o falla el envío, NO rompe la compra: solo devuelve false.
    public class CorreoService
    {
        private readonly IConfiguration _config;
        public CorreoService(IConfiguration config) { _config = config; }

        public bool Enviar(string destino, string asunto, string html)
        {
            try
            {
                var c = _config.GetSection("Correo");
                var usuario = c["Usuario"];
                var clave = c["Clave"];
                if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave)) return false;

                using var smtp = new SmtpClient(c["Servidor"] ?? "smtp.gmail.com", int.Parse(c["Puerto"] ?? "587"))
                {
                    Credentials = new NetworkCredential(usuario, clave),
                    EnableSsl = true
                };
                using var msg = new MailMessage
                {
                    From = new MailAddress(usuario, "Playeras JyC"),
                    Subject = asunto,
                    Body = html,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };
                msg.To.Add(destino);
                smtp.Send(msg);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar correo: " + ex.Message);
                return false;
            }
        }

        public bool EnviarTicket(Pedido p, string? destino = null)
        {
            return Enviar(destino ?? p.Usuario.Email,
                "Tu ticket Playeras JyC " + p.Folio,
                CuerpoTicket(p));
        }

        public static string CuerpoTicket(Pedido p)
        {
            string H(string? s) => WebUtility.HtmlEncode(s ?? "");
            var sb = new StringBuilder();
            sb.Append("<div style='font-family:Arial,sans-serif;max-width:620px;margin:auto;border:1px solid #ddd;padding:20px'>");
            sb.Append("<h2 style='color:#ff007f;margin:0'>PLAYERAS <span style='color:#00a8b5'>JyC</span></h2>");
            sb.Append("<p style='color:#666;margin-top:4px'>Comprobante de compra</p>");
            sb.Append("<p><b>Folio:</b> " + H(p.Folio) + "<br>");
            sb.Append("<b>Fecha:</b> " + p.FechaHora.ToString("dd/MM/yyyy HH:mm") + "<br>");
            sb.Append("<b>Cliente:</b> " + H(p.Usuario.Nombre) + "<br>");
            sb.Append("<b>Estado:</b> " + H(p.EstadoPedido) + "</p>");

            sb.Append("<p><b>Envío a:</b> " + H(p.NombreDestinatario) + "<br>" + H(p.DireccionEnvio) + ", " + H(p.Ciudad) + " C.P. " + H(p.CodigoPostal) + "</p>");

            sb.Append("<table style='width:100%;border-collapse:collapse'>");
            sb.Append("<tr style='background:#f2f2f2'><th align='left' style='padding:6px'>Producto</th><th>Cant.</th><th align='right'>Precio</th><th align='right'>Importe</th></tr>");
            foreach (var d in p.Detalles)
            {
                sb.Append("<tr><td style='padding:6px;border-bottom:1px solid #eee'>" + H(d.NombreProducto) + " <small>(" + H(d.Talla) + " / " + H(d.Color) + ")</small></td>");
                sb.Append("<td align='center' style='border-bottom:1px solid #eee'>" + d.Cantidad + "</td>");
                sb.Append("<td align='right' style='border-bottom:1px solid #eee'>$" + d.PrecioUnitario.ToString("N2") + "</td>");
                sb.Append("<td align='right' style='border-bottom:1px solid #eee'>$" + (d.Cantidad * d.PrecioUnitario).ToString("N2") + "</td></tr>");
            }
            sb.Append("</table>");

            sb.Append("<p style='text-align:right'>Subtotal: $" + p.Subtotal.ToString("N2") + "<br>");
            if (p.Descuento > 0) sb.Append("Descuento: -$" + p.Descuento.ToString("N2") + "<br>");
            sb.Append("Envío: $" + p.CostosEnvio.ToString("N2") + "<br>");
            sb.Append("<b style='font-size:18px'>Total: $" + p.Total.ToString("N2") + " MXN</b></p>");
            sb.Append("<p><b>Método de pago:</b> " + H(p.MetodoPago) + "</p>");
            sb.Append("<p style='color:#777'>Gracias por tu compra en Playeras JyC.</p></div>");
            return sb.ToString();
        }
    }
}
