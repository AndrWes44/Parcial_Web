using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace Proyecto.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreUsuario { get; set; }

        [Required]
        public string Correo { get; set; }

        [Required]
        public string Contrasena { get; set; }

        public string? Telefono { get; set; }

        public string? Tipo { get; set; }

        [ForeignKey("Rol")]
        public int RolId { get; set; }
        public Rol Rol { get; set; }

        public string? Empresa { get; set; }
        public string? Contacto { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }

        public ICollection<Ticket> TicketsCreados { get; set; }
        public ICollection<Asignacion> Asignaciones { get; set; }
        public ICollection<Comentario> Comentarios { get; set; }
    }
}
