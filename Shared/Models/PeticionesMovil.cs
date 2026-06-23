using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class PeticionMovil
    {
        public int UsuarioID { get; set; }
        public int LaboratorioID { get; set; }
        public string FechaPrestamo { get; set; }
    }
}