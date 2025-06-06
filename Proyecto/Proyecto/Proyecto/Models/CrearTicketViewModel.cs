namespace Proyecto.Models
{
    public class CrearTicketViewModel
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int CategoriaId { get; set; }

        // Para capturar IDs de técnicos seleccionados (multiple select)
        public List<int> AsignadoTecnicosIds { get; set; }
    }
}
