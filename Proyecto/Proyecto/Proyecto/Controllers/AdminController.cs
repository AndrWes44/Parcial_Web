using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace Proyecto.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        public AdminController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        // === 1. DASHBOARD ===
        public IActionResult Index()
        {
            var totalTickets = _context.Ticket.Count();

            var ticketsPorEstado = _context.Ticket
                .GroupBy(t => t.Estado)
                .Select(g => new
                {
                    Estado = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            var ticketsPorCategoria = _context.Ticket
                .Include(t => t.Categoria)
                .GroupBy(t => t.Categoria.Nombre)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            ViewBag.TotalTickets = totalTickets;
            ViewBag.TicketsPorEstado = ticketsPorEstado;
            ViewBag.TicketsPorCategoria = ticketsPorCategoria;

            return View();
        }

        // === 2. GESTIÓN DE TICKETS ===

        public IActionResult Tickets()
        {
            var tickets = _context.Ticket
                .Include(t => t.Categoria)
                .Include(t => t.CreadoPor)
                .Include(t => t.Asignaciones)
                    .ThenInclude(a => a.Tecnico) // Asumiendo que tienes la tabla Asignaciones
                .ToList();

            return View(tickets);
        }

        [HttpGet]
        public IActionResult CrearTicket()
        {
            ViewBag.Categorias = new SelectList(_context.Categoria.Where(c => c.Estado == "Activo"), "Id", "Nombre");
            ViewBag.Usuarios = new SelectList(_context.Usuario.Where(u => u.Rol.Nombre == "Cliente"), "Id", "NombreUsuario");
            ViewBag.Tecnicos = new SelectList(_context.Usuario.Where(u => u.Rol.Nombre == "Técnico"), "Id", "NombreUsuario");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearTicket(Ticket ticket, int? TecnicoId, IFormFile ArchivoAdjunto)
        {

            ticket.Estado = "Abierto";
                    ticket.FechaCreacion = DateTime.Now;
                    ticket.FechaActualizacion = DateTime.Now;

                    _context.Ticket.Add(ticket);
                    _context.SaveChanges();


            if (TecnicoId.HasValue && TecnicoId.Value > 0)
            {
                // Crear asignación del técnico
                var asignacion = new Asignacion
                {
                    TicketId = ticket.Id,
                    TecnicoId = TecnicoId.Value,
                    FechaAsignacion = DateTime.Now
                };
                _context.Asignacion.Add(asignacion);
                _context.SaveChanges();

                var ticketDb = _context.Ticket.Include(t => t.Asignaciones).FirstOrDefault(t => t.Id == ticket.Id);

                if (ticketDb == null)
                {
                    return NotFound();
                }

                var tecnico = _context.Usuario.FirstOrDefault(u => u.Id == TecnicoId.Value);
                if (tecnico != null)
                {
                    string asunto = $"Asignación de Ticket #{ticket.Id}";
                    string cuerpo = $"Hola {tecnico.NombreUsuario},<br/><br/>" +
                                    $"Se te ha asignado el ticket con ID <strong>#{ticket.Id}</strong>.<br/>" +
                                    $"<strong>Título:</strong> {ticketDb.Titulo}<br/>" +
                                    $"<strong>Prioridad:</strong> {ticketDb.Prioridad}<br/>" +
                                    $"<strong>Fecha de asignación:</strong> {DateTime.Now}<br/><br/>" +
                                    $"Por favor, ingresa al sistema para gestionarlo.<br/><br/>" +
                                    $"Gracias,<br/>Equipo de soporte.";

                    _emailService.EnviarCorreo(tecnico.Correo, asunto, cuerpo);
                }
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

            return RedirectToAction("Tickets");
        }

        public IActionResult EditarTicket(int id)
        {
            var ticket = _context.Ticket.Find(id);
            if (ticket == null) return NotFound();
            ViewBag.Categorias = _context.Categoria.ToList();
            ViewBag.Usuarios = _context.Usuario.ToList();
            return View(ticket);
        }

        [HttpPost]
        public IActionResult EditarTicket(Ticket ticket)
        {
            var ticketExistente = _context.Ticket.FirstOrDefault(t => t.Id == ticket.Id);
            if (ticketExistente == null) return NotFound();

            // Validación: ¿la categoría existe?
            var categoriaExiste = _context.Categoria.Any(c => c.Id == ticket.CategoriaId);
            if (!categoriaExiste)
            {
                ModelState.AddModelError("CategoriaId", "La categoría seleccionada no existe.");
                ViewBag.Categorias = _context.Categoria.ToList();
                return View(ticket);
            }

            ticketExistente.Titulo = ticket.Titulo;
            ticketExistente.Descripcion = ticket.Descripcion;
            ticketExistente.Prioridad = ticket.Prioridad;
            ticketExistente.Estado = ticketExistente.Estado;
            ticketExistente.CategoriaId = ticket.CategoriaId;
            ticketExistente.FechaActualizacion = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Tickets");
        }

        public IActionResult DetalleTicket(int id)
        {
            var ticket = _context.Ticket
                .Include(t => t.Categoria)
                .Include(t => t.CreadoPor)
                .Include(t => t.Comentarios).ThenInclude(c => c.Usuario)
                .Include(t => t.Asignaciones).ThenInclude(a => a.Tecnico)
                .Include(t => t.Adjuntos)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // Lista de usuarios técnicos (o todos los usuarios)
            var usuarios = _context.Usuario
                .Include(u => u.Rol)
                .Where(u => u.Rol.Nombre == "Técnico")
                .ToList();

            ViewBag.Usuarios = usuarios;

            return View(ticket);
        }
        [HttpPost]
        public IActionResult ActualizarTicket(Ticket ticket, int? AsignadoAId)
        {
            var ticketDb = _context.Ticket.Include(t => t.Asignaciones).FirstOrDefault(t => t.Id == ticket.Id);

            if (ticketDb == null)
            {
                return NotFound();
            }

            // Guardar estado anterior para historial (opcional)
            var estadoAnterior = ticketDb.Estado;

            ticketDb.Estado = ticket.Estado;
            ticketDb.FechaActualizacion = DateTime.Now;

            _context.SaveChanges();

            // Agregar entrada al historial (si tienes esa lógica implementada)
            var historial = new HistorialEstado
            {
                TicketId = ticket.Id,
                EstadoAnterior = estadoAnterior,
                NuevoEstado = ticket.Estado,
                CambiadoPorId = HttpContext.Session.GetInt32("UsuarioId") ?? 0,
                FechaCambio = DateTime.Now
            };

            _context.HistorialEstado.Add(historial);
            _context.SaveChanges();

            var creador = _context.Usuario.FirstOrDefault(u => u.Id == ticketDb.CreadoPorId);

            // Agregar asignación si aún no existe y se seleccionó técnico
            if (AsignadoAId.HasValue)
            {
                var ultimaAsignacion = ticketDb.Asignaciones
                    .OrderByDescending(a => a.FechaAsignacion)
                    .FirstOrDefault();

                if (ultimaAsignacion == null || ultimaAsignacion.TecnicoId != AsignadoAId.Value)
                {
                    if (ultimaAsignacion != null)
                    {
                        var tecnicoAnterior = _context.Usuario.FirstOrDefault(u => u.Id == ultimaAsignacion.TecnicoId);
                        if (tecnicoAnterior != null)
                        {
                            string asuntoAnterior = $"Ticket #{ticket.Id} ha sido reasignado";
                            string cuerpoAnterior = $"Hola {tecnicoAnterior.NombreUsuario},<br/><br/>" +
                                $"El ticket con ID <strong>#{ticket.Id}</strong> que estaba asignado a ti ha sido reasignado a otro técnico.<br/>" +
                                $"<strong>Título:</strong> {ticketDb.Titulo}<br/>" +
                                $"<strong>Fecha de reasignación:</strong> {DateTime.Now}<br/><br/>" +
                                $"Gracias por tu gestión.<br/><br/>" +
                                $"Equipo de Soporte.";

                            _emailService.EnviarCorreo(tecnicoAnterior.Correo, asuntoAnterior, cuerpoAnterior);
                        }

                        //Eliminar asignación anterior
                        _context.Asignacion.Remove(ultimaAsignacion);
                        _context.SaveChanges();
                    }

                    var nuevaAsignacion = new Asignacion
                    {
                        TicketId = ticket.Id,
                        TecnicoId = AsignadoAId.Value,
                        FechaAsignacion = DateTime.Now
                    };
                    _context.Asignacion.Add(nuevaAsignacion);
                    _context.SaveChanges();

                    var tecnico = _context.Usuario.FirstOrDefault(u => u.Id == AsignadoAId.Value);
                    if (tecnico != null)
                    {
                        string asunto = $"Asignación de Ticket #{ticket.Id}";
                        string cuerpo = $"Hola {tecnico.NombreUsuario},<br/><br/>" +
                                        $"Se te ha asignado el ticket con ID <strong>#{ticket.Id}</strong>.<br/>" +
                                        $"<strong>Título:</strong> {ticketDb.Titulo}<br/>" +
                                        $"<strong>Prioridad:</strong> {ticketDb.Prioridad}<br/>" +
                                        $"<strong>Fecha de asignación:</strong> {DateTime.Now}<br/><br/>" +
                                        $"Por favor, ingresa al sistema para gestionarlo.<br/><br/>" +
                                        $"Gracias,<br/>Equipo de soporte.";

                        _emailService.EnviarCorreo(tecnico.Correo, asunto, cuerpo);
                    }
                }
            }
            if (creador != null)
            {
                string asuntoCreador = $"Actualización del Ticket #{ticket.Id}";
                string cuerpoCreador = $"Hola {creador.NombreUsuario},<br/><br/>" +
                    $"Tu ticket <strong>#{ticket.Id}</strong> ha sido actualizado.<br/>" +
                    $"<strong>Nuevo estado:</strong> {ticket.Estado}<br/>" +
                    (AsignadoAId.HasValue ? "<strong>Ha sido asignado a un técnico.</strong><br/><br/>" : "") +
                    $"Gracias por utilizar nuestro sistema.<br/><br/>" +
                    $"Saludos,<br/>Equipo de Soporte";

                _emailService.EnviarCorreo(creador.Correo, asuntoCreador, cuerpoCreador);
            }
            return RedirectToAction("DetalleTicket", new { id = ticket.Id });
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

        [HttpPost]
        public IActionResult AgregarComentario(int TicketId, string Contenido)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var comentario = new Comentario
            {
                TicketId = TicketId,
                UsuarioId = usuarioId.Value,
                Contenido = Contenido,
                FechaCreacion = DateTime.Now
            };

            _context.Comentario.Add(comentario);
            _context.SaveChanges();

            return RedirectToAction("DetalleTicket", new { id = TicketId });
        }

        public IActionResult HistorialTicket(int id)
        {
            var cambios = _context.HistorialEstado
        .Include(h => h.CambiadoPor)
        .Where(h => h.TicketId == id)
        .OrderByDescending(h => h.FechaCambio)
        .ToList();

            var comentarios = _context.Comentario
                .Include(c => c.Usuario)
                .Where(c => c.TicketId == id)
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();

            var viewModel = new TicketHistorialViewModel
            {
                TicketId = id,
                CambiosEstado = cambios,
                Comentarios = comentarios
            };

            return View(viewModel);

        }


        public IActionResult EliminarTicket(int id)
        {
            var ticket = _context.Ticket
                .Include(t => t.Asignaciones)
                .Include(t => t.Comentarios)
                .Include(t => t.Adjuntos)
                .Include(t => t.HistorialEstados)
                .Include(t => t.Notificaciones)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null) return NotFound();

            // Eliminar entidades relacionadas primero
            _context.Asignacion.RemoveRange(ticket.Asignaciones);
            _context.Comentario.RemoveRange(ticket.Comentarios);
            _context.Adjunto.RemoveRange(ticket.Adjuntos);
            _context.HistorialEstado.RemoveRange(ticket.HistorialEstados);
            _context.Notificacion.RemoveRange(ticket.Notificaciones);

            // Luego eliminar el ticket
            _context.Ticket.Remove(ticket);
            _context.SaveChanges();

            return RedirectToAction("Tickets");
        }

        // === 3. GESTIÓN DE USUARIOS ===

        public IActionResult Usuarios()
        {
            var usuarios = _context.Usuario.Include(u => u.Rol).ToList();
            return View(usuarios);
        }

        public async Task<IActionResult> CrearUsuario()
        {
            ViewBag.Roles = new SelectList(await _context.Rol.ToListAsync(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(Usuario usuario)
        {
            ViewBag.Roles = new SelectList(await _context.Rol.ToListAsync(), "Id", "Nombre");
            usuario.FechaCreacion = DateTime.Now;
            _context.Usuario.Add(usuario);
            _context.SaveChanges();

            // Enviar notificación por correo
            string asunto = "Bienvenido al Sistema de Tickets";
            string cuerpo = $"Hola {usuario.NombreUsuario},<br/>" +
                            "Tu cuenta ha sido creada exitosamente.<br/>" +
                            $"Usuario: {usuario.Correo}<br/>" +
                            $"Contraseña: {usuario.Contrasena}<br/>" +
                            "No compartas esta informacion con nadie.<br/>" +
                            "Ya puedes iniciar sesión con tu usuario. Bienvenido.";

            _emailService.EnviarCorreo(usuario.Correo, asunto, cuerpo);

            return RedirectToAction("Usuarios");
        }
        public async Task<IActionResult> EditarUsuario(int id)
        {
            var usuario = _context.Usuario.Find(id);
            if (usuario == null) return NotFound();
            ViewBag.Roles = new SelectList(await _context.Rol.ToListAsync(), "Id", "Nombre", usuario.RolId);
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> EditarUsuario(Usuario usuario)
        {
            ViewBag.Roles = new SelectList(await _context.Rol.ToListAsync(), "Id", "Nombre", usuario.RolId);
            usuario.FechaActualizacion = DateTime.Now;
            _context.Usuario.Update(usuario);
            _context.SaveChanges();
            return RedirectToAction("Usuarios");
        }
        [HttpGet]
        public IActionResult DashboardGraficas()
        {
            var ticketsPorEstado = _context.Ticket
                .GroupBy(t => t.Estado)
                .Select(g => new {
                    Estado = g.Key,
                    Total = g.Count()
                }).ToList();

            var ticketsPorPrioridad = _context.Ticket
                .GroupBy(t => t.Prioridad)
                .Select(g => new {
                    Prioridad = g.Key,
                    Total = g.Count()
                }).ToList();

            var ticketsPorMes = _context.Ticket
                .GroupBy(t => new { t.FechaCreacion.Year, t.FechaCreacion.Month })
                .Select(g => new {
                    Mes = g.Key.Month,
                    Anio = g.Key.Year,
                    Total = g.Count()
                }).OrderBy(g => g.Anio).ThenBy(g => g.Mes)
                .ToList();

            return Json(new
            {
                estados = ticketsPorEstado,
                prioridades = ticketsPorPrioridad,
                porMes = ticketsPorMes
            });
        }
        public IActionResult EliminarUsuario(int id)
        {
            var usuario = _context.Usuario.Find(id);
            if (usuario == null) return NotFound();
            _context.Usuario.Remove(usuario);
            _context.SaveChanges();
            return RedirectToAction("Usuarios");
        }
    }
}
