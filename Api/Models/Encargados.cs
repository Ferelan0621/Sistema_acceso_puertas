using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Models
{
    public class Encargados
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = null!;
        public string password { get; set; } = null!;
        public string Estatus { get; set; }

        
    }
}
