using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        public ClienteController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            int idCliente = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var tickets = _context.Ticket
                .Where(t => t.CreadoPorId == idCliente)
                .Include(t => t.Categoria)
                .ToList();

            return View(tickets);
        }

        [HttpGet]
        public IActionResult CrearTicket()
        {
            ViewBag.Categorias = new SelectList(_context.Categoria.Where(c => c.Estado == "Activo"), "Id", "Nombre");
            ViewBag.Usuarios = new SelectList(_context.Usuario.Where(u => u.Rol.Nombre == "Cliente"), "Id", "NombreUsuario");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearTicket(Ticket ticket, IFormFile ArchivoAdjunto)
        {

            ticket.Estado = "Abierto";
            ticket.FechaCreacion = DateTime.Now;
            ticket.FechaActualizacion = DateTime.Now;

            _context.Ticket.Add(ticket);
            _context.SaveChanges();

            var usuario = _context.Usuario.FirstOrDefault(u => u.Id == ticket.CreadoPorId);
            if (usuario != null)
            {
                string asunto = "Confirmación de Creación de Ticket";
                string cuerpo = $"Hola {usuario.NombreUsuario},<br/>" +
                                $"Se ha creado tu ticket con ID <strong>#{ticket.Id}</strong>.<br/>" +
                                $"Título: {ticket.Titulo}<br/>" +
                                $"Estado: {ticket.Estado}<br/>" +
                                $"Fecha: {ticket.FechaCreacion}<br/><br/>" +
                                $"Gracias por contactarnos.";

                _emailService.EnviarCorreo(usuario.Correo, asunto, cuerpo);
            }

            // Verificar si se subió un archivo
            if (ArchivoAdjunto != null && ArchivoAdjunto.Length > 0)
            {
                var nombreArchivo = Path.GetFileName(ArchivoAdjunto.FileName);
                var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "adjuntos");
                Directory.CreateDirectory(rutaCarpeta); // Asegura que la carpeta exista

                var rutaArchivo = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    ArchivoAdjunto.CopyTo(stream);
                }

                // Guardar registro en la tabla Adjunto
                var adjunto = new Adjunto
                {
                    TicketId = ticket.Id, // ID ya fue generado con SaveChanges()
                    NombreArchivo = nombreArchivo,
                    RutaArchivo = "/adjuntos/" + nombreArchivo,
                    FechaSubida = DateTime.Now
                };

                _context.Adjunto.Add(adjunto);
                _context.SaveChanges();
            }

            TempData["MensajeExito"] = "✅ Ticket creado exitosamente.";
            return RedirectToAction("Index");
        }
        public IActionResult Comentar(int id)
        {
            ViewBag.TicketId = id;
            return View();
        }

        [HttpPost]
        public IActionResult Comentar(int ticketId, string contenido)
        {
            _context.Comentario.Add(new Comentario
            {
                TicketId = ticketId,
                UsuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0,
                Contenido = contenido,
                FechaCreacion = DateTime.Now
            });

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DetalleTicket(int id)
        {
            int idCliente = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            var ticket = _context.Ticket
                .Include(t => t.Categoria)
                .Include(t => t.Comentarios)
                    .ThenInclude(c => c.Usuario)
                .Include(t => t.Adjuntos)
                .FirstOrDefault(t => t.Id == id && t.CreadoPorId == idCliente);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarAdjunto(int TicketId, IFormFile ArchivoAdjunto)
        {
            if (ArchivoAdjunto != null && ArchivoAdjunto.Length > 0)
            {
                var nombreArchivo = Path.GetFileName(ArchivoAdjunto.FileName);
                var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "adjuntos");
                Directory.CreateDirectory(rutaCarpeta);

                var rutaArchivo = Path.Combine(rutaCarpeta, nombreArchivo);
                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    ArchivoAdjunto.CopyTo(stream);
                }

                var adjunto = new Adjunto
                {
                    TicketId = TicketId,
                    NombreArchivo = nombreArchivo,
                    RutaArchivo = "/adjuntos/" + nombreArchivo,
                    FechaSubida = DateTime.Now
                };

                _context.Adjunto.Add(adjunto);
                _context.SaveChanges();
            }

            return RedirectToAction("DetalleTicket", new { id = TicketId });
        }
    }
}
