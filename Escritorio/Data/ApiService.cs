using Shared.Models;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading;

namespace Escritorio.Data
{
    internal class ApiService
    {
        // o 10.0.2.2 si usas el emulador de Android.
        private readonly HttpClient _httpClient;
        private const string BaseUrl = ConexionHTTP.BaseUrl;


        public ApiService()
        {
            // Para desarrollo local con certificados HTTPS autofirmados en Android,
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
        public async Task EscucharPuertasSSEAsync(Action<SensorPuerta> onMensajeRecibido, CancellationToken cancellationToken)
        {
            try
            {
                // OJO: Cambia esta ruta por la URL real de tu API que emite los eventos SSE
                using var request = new HttpRequestMessage(HttpMethod.Get, "Laboratorios/stream-puertas");

                // Le decimos al servidor que queremos mantener la conexión abierta recibiendo eventos
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

                // HttpCompletionOption.ResponseHeadersRead es CLAVE para que no espere a que acabe la petición
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);

                // Bucle infinito que lee línea por línea mientras no se cancele
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // SSE estándar manda los datos empezando con "data: "
                    if (line.StartsWith("data: "))
                    {
                        var json = line.Substring(6); // Cortamos la palabra "data: "

                        try
                        {
                            var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var evento = JsonSerializer.Deserialize<SensorPuerta>(json, opcionesJson);

                            if (evento != null)
                            {
                                // Disparamos la acción de vuelta al ViewModel
                                onMensajeRecibido?.Invoke(evento);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error parseando JSON del SSE: {ex.Message}");
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Conexión SSE terminada a propósito.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de conexión SSE: {ex.Message}");
                // Aquí podrías agregar un Task.Delay y volver a llamar a la función para autoconectar
            }
        }
    }
}
