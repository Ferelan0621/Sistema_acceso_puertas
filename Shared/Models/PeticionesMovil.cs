using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization; // ¡CRÍTICO! Agregado para usar las etiquetas

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
	}
}