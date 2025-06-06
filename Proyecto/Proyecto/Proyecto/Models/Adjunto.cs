using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto.Models
{
    public class Adjunto
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public string RutaArchivo { get; set; }

        public string NombreArchivo { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.Now;
    }
}
