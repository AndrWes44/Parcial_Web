using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto.Models
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public string Mensaje { get; set; }

        public string Tipo { get; set; }  // Info, Error, Advertencia, etc.

        public bool Leida { get; set; } = false;

        public DateTime FechaEnvio { get; set; } = DateTime.Now;
    }
}
