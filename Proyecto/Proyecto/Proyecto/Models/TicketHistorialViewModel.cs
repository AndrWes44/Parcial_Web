namespace Proyecto.Models
{
    public class TicketHistorialViewModel
    {
        public int TicketId { get; set; }
        public List<HistorialEstado> CambiosEstado { get; set; }
        public List<Comentario> Comentarios { get; set; }
    }
}
