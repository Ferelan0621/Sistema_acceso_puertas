
namespace Shared.Models
{
    public class Prestamos
    {
        public int ID { get; set; }

        // Clave foránea hacia el usuario que creó la petición
        public int UsuarioID { get; set; }

        // Propiedad de navegación hacia el usuario que la creó
        public Usuarios Usuario { get; set; } = null!;

        public int LaboratorioID { get; set; }
        public Laboratorios Laboratorio { get; set; } = null!;


        public DateTime FechaPrestamo { get; set; } 
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFinal { get; set; }
        public int EncargadoID { get; set; }
        public Encargados Encargado { get; set; } = null!;

    }
}
