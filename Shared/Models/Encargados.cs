using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class Encargados
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = null!;
        public string Password { get; set; } = null!;
        public required string Estatus { get; set; }


    }
}
