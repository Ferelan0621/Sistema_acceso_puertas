
namespace Shared.Models
{
    public class Prestamos
    {
        public int Id { get; set; }

        // Clave foránea hacia el usuario que creó la petición
        public int Usuario_Id { get; set; }

        // Propiedad de navegación hacia el usuario que la creó
        public Usuarios Usuario { get; set; } = null!;

        public int Laboratodio_Id { get; set; }
        public Laboratorios Laboratorios { get; set; }=null!;

        public ICollection<Encargados> Encargados { get; set; } = new List<Encargados>();

        public DateTime Fecha_Prestamo { get; set; }
        public DateTime Hora_Inicio { get; set; }   
        public DateTime Hora_Final { get; set; }    
        public bool Abierto { get; set; }
        
    }
}
