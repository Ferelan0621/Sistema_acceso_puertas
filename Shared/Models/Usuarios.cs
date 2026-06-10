    using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Shared.Models
{
    public class Usuarios
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ClaveISSEMYM { get; set; } = null!;
        public Rol Rol { get; set; }

        // Relación uno a muchos con Prestamos
        public ICollection<Prestamos> Prestamos { get; set; } = new List<Prestamos>();
    }
    public class LoginRequest
    {
        public string? ClaveISSEMYM { get; set; }
        public string? Password { get; set; }
    }
    public class RespuestaLogin
    {
        // Tienen que llamarse igual que en tu API
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("usuarioID")]
        public int usuarioID { get; set; } // Asumiendo que el ID en tu BD es numérico
        [JsonPropertyName("nombreUser")]
        public string nombreUser { get; set; }
    }
}
