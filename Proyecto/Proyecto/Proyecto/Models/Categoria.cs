using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace Proyecto.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Estado { get; set; } = "Abierto";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaActualizacion { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
    }
}
