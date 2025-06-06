using System.ComponentModel.DataAnnotations;

namespace Proyecto.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }
    }
}
