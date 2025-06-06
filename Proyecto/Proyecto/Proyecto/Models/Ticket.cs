using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        public string? Descripcion { get; set; }

        public string? Prioridad { get; set; }

        public string? Estado { get; set; }

        [ForeignKey("Categoria")]
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        [ForeignKey("CreadoPor")]
        public int CreadoPorId { get; set; }
        public Usuario CreadoPor { get; set; }

        public DateTime FechaCreacion { get; set; } 
        public DateTime? FechaActualizacion { get; set; }
        public ICollection<Asignacion> Asignaciones { get; set; }

        public ICollection<Comentario> Comentarios { get; set; }
        public ICollection<Adjunto> Adjuntos { get; set; }
        public ICollection<HistorialEstado> HistorialEstados { get; set; }
        public ICollection<Notificacion> Notificaciones { get; set; }
    }
}
