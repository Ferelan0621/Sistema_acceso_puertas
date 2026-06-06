using Shared.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace Escritorio.Data
{
    internal class ApiService
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

        public async Task<bool> IniciarSesionAsync(string usuario, string password)
        {
            try
            {
                // Creamos el objeto con los datos del formulario
                var datosLogin = new
                {
                    Nombre = usuario,
                    Password = password
                };

                // Hacemos la petición POST enviando el JSON en el cuerpo
                var response = await _httpClient.PostAsJsonAsync("Encargados/login", datosLogin);

                if (response.IsSuccessStatusCode)
                {
                    // El servidor respondió con 200 OK
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
    }
}
