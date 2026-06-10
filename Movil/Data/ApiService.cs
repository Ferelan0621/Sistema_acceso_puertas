using Azure;
using Shared.Models;
using Shared.Services;
using System.Buffers.Text;
using System.Net.Http.Json;
using System.Text.Json;

namespace Movil.Services
{
    public class ApiService
    {
        // Cambia esta URL por la IP de tu máquina en tu red local si pruebas en un dispositivo físico
        // o 10.0.2.2 si usas el emulador de Android.
        private readonly HttpClient _httpClient;
        private const string BaseUrl = ConexionHTTP.BaseUrl;


        public ApiService()
        {
            // Nota: Para desarrollo local con certificados HTTPS autofirmados en Android,
            // necesitas un HttpClientHandler especial que ignore los errores de SSL.
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        }

     

        // 4. RECORDAR / RESTABLECER CONTRASEÑA
        public async Task<bool> RecuperarContrasenaAsync(string claveImssemym, string nuevaPassword)
        {
            var datosRecuperacion = new { ClaveISSEMYM = claveImssemym, NuevaPassword = nuevaPassword };

            // Apuntamos a la ruta "Usuarios/Recuperar" que creamos en el API
            var response = await _httpClient.PostAsJsonAsync("Usuarios/Recuperar", datosRecuperacion);

            return response.IsSuccessStatusCode;
        }


        public async Task<bool> IniciarSesionAsync(string clave, string password)
        {
            try
            {
                // Creamos el objeto con los datos del formulario
                var datosLogin = new
                {
                    ClaveISSEMYM = clave,
                    Password = password
                };

                // Hacemos la petición POST enviando el JSON en el cuerpo
                var response = await _httpClient.PostAsJsonAsync("Usuarios/login", datosLogin);
                if (response.IsSuccessStatusCode)
                {
                    // Deserializamos el objeto que retorna tu API
                    var resultado = await response.Content.ReadFromJsonAsync<RespuestaLogin>();

                    if (resultado != null)
                    
                        // 3. Almacenamos los datos según su sensibilidad

                        // Guardamos el ID y Nombre (Datos básicos)
                       Preferences.Default.Set("usuarioID", resultado.usuarioID);
                       Preferences.Default.Set("nombreUser", resultado.nombreUser);

                        // Si el login tuviera un Token, aquí usarías SecureStorage.Default.SetAsync(...)

                        
                    
                        return true;
                }
                else
                {
                    // Aquí puedes leer el error si el servidor mandó un BadRequest o Unauthorized
                    string errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error del servidor: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Aquí atrapas fallas de red, Dev Tunnel caído, o temas de SSL
                System.Diagnostics.Debug.WriteLine($"Error de conexión: {ex.Message}");
                return false;
            }
       }

        public async Task<List<Laboratorios>> ObtenerLaboratoriosAsync()
        {
            try
            {
                // Configuración vital para que empate el camelCase del JSON con el PascalCase de C#
                var opcionesJson = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var respuesta = await _httpClient.GetAsync("Laboratorios");
                respuesta.EnsureSuccessStatusCode();

                var contenido = await respuesta.Content.ReadAsStringAsync();
                var elementos = JsonSerializer.Deserialize<List<Laboratorios>>(contenido, opcionesJson);

                return elementos ?? new List<Laboratorios>();
            }
            catch (Exception ex)
            {
                // Aquí puedes manejar el error (ej. mostrar una alerta, logs)
                Console.WriteLine($"Error en la API: {ex.Message}");
                return new List<Laboratorios>();
            }
        }


    }
}