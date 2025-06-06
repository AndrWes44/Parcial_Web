using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto.Models
{
    public class Asignacion
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        [ForeignKey("Tecnico")]
        public int TecnicoId { get; set; }
        public Usuario Tecnico { get; set; }

        public DateTime FechaAsignacion { get; set; }
    }
}
