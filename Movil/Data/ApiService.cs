using Shared.Models;
using Shared.Services;
using System.Net.Http.Json;

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

        // 1. LOGIN
        public async Task<Usuarios> LoginAsync(string nombre, string password)
        {
            var loginData = new { Clave_ISSEMYM = nombre, Password = password };
            var response = await _httpClient.PostAsJsonAsync("Usuarios/Login", loginData);

            if (response.IsSuccessStatusCode)
            {

                return await response.Content.ReadFromJsonAsync<Usuarios>();

            }
            else
            {
                return null;
            }           
        }


        // 2. CREAR USUARIO
        // 4. RECORDAR / RESTABLECER CONTRASEÑA
        public async Task<bool> RecuperarContrasenaAsync(string claveImssemym, string nuevaPassword)
        {
            var datosRecuperacion = new { Clave_ISSEMYM = claveImssemym, NuevaPassword = nuevaPassword };

            // Apuntamos a la ruta "Usuarios/Recuperar" que creamos en el API
            var response = await _httpClient.PostAsJsonAsync("/Usuarios/Recuperar", datosRecuperacion);

            return response.IsSuccessStatusCode;
        }

        public async Task<Usuarios> VerUserasync()
        {
            var response = await _httpClient.GetAsync("/Usuario");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Usuarios>();
            }

            return null;
        }
    }
}