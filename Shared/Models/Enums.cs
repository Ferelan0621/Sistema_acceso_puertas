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
       
        Administrativo = 1,
        Docente = 2,
        Mantenimiento = 3,
        Intendencia = 4
    }

}
