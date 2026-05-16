using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Shared.Models
{
    
    public enum EstadoLaboratorio
    {
        Disponible,
        Ocupado,
        Mantenimiento,
        Limpieza
    }

    public enum Rol
    {
        Administrador,
        Administrativo,
        Docente,
        Mantenimiento,
        Intendencia
    }
}
