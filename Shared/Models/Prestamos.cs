
namespace Shared.Models
{
    public class Prestamos
    {
        public int ID { get; set; }

        // Clave foránea hacia el usuario que creó la petición
        public int UsuarioID { get; set; }

        // Propiedad de navegación hacia el usuario que la creó
        public Usuarios Usuario { get; set; } = null!;

        public int LaboratodioID { get; set; }
        public Laboratorios Laboratorios { get; set; }=null!;


        public DateTime Fecha_Prestamo { get; set; }
        public DateTime Hora_Inicio { get; set; }   
        public DateTime Hora_Final { get; set; }    
        public int EncargadoID { get; set; }
        public Encargados Encargado { get; set; } = null!;

    }
}
