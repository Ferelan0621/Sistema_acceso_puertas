using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Shared.Models
{

    public enum EstadoLaboratorio
    {
        Disponible = 0,
        Ocupado = 1,
        Mantenimiento = 2,
        Limpieza = 3
    }

    public enum Rol
    {
        Administrador = 0,
        Administrativo = 1,
        Docente = 2,
        Mantenimiento = 3,
        Intendencia = 4
    }

}
namespace Shared.Models
{
    public class RolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class RegistroUsuarioDTO
    {
        public string Nombre { get; set; }
        public string ClaveIMSSEMYM { get; set; }
        public string Password { get; set; }
        public int RolId { get; set; }
    }
}