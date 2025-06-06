using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto.Models
{
    public class HistorialEstado
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public string EstadoAnterior { get; set; }

        public string NuevoEstado { get; set; }

        [ForeignKey("CambiadoPor")]
        public int CambiadoPorId { get; set; }
        public Usuario CambiadoPor { get; set; }

        public DateTime FechaCambio { get; set; } = DateTime.Now;
    }
}
