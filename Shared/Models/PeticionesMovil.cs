using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization; 

namespace Shared.Models
{
	public class PeticionMovil
	{
		[JsonPropertyName("usuarioID")]
		public int UsuarioID { get; set; }

		[JsonPropertyName("laboratorioID")]
		public int LaboratorioID { get; set; }

		[JsonPropertyName("fechaPrestamo")]
		public string FechaPrestamo { get; set; }

		public string NombreUsuario { get; set; }
		public string Cargo { get; set; }
	}
}