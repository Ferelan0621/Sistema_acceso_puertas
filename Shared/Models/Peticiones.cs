
namespace Shared.Models
{
    public class Peticiones
    {
        public int Id { get; set; }

        // Clave foránea hacia el usuario que creó la petición
        public int Usuario_Id { get; set; }

        // Propiedad de navegación hacia el usuario que la creó
        public Usuarios Usuario { get; set; } = null!;

        public int Laboratodio_Id { get; set; }
        public Laboratorios Laboratorios { get; set; }=null!;
        public int Respuesta_Id { get; set; }

        // RELACIÓN 1 A 1: Una petición tiene UNA SOLA respuesta (o ninguna todavía)
        public Respuesta? Respuesta { get; set; }
        public DateTime Fecha_Peticion { get; set; }
        public DateTime Hora_Inicio { get; set; }   
        public DateTime Hora_Final { get; set; }    
        public bool Abierto { get; set; }
        
    }
}
