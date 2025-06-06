using System.ComponentModel.DataAnnotations;

namespace Proyecto.Models
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Permisos { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaActualizacion { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
    }
}
