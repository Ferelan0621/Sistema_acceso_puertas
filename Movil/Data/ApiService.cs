using Azure;
using Shared.Models;
using Shared.Services;
using System.Buffers.Text;
using System.Net.Http.Headers;
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

        private readonly JsonSerializerOptions _jsonOptions;

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

        public async Task EscucharActualizacionesSSEAsync(Action<List<Laboratorios>> alActualizar, CancellationToken token)
        {
            var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // El handler y el HttpClient se instancian una sola vez afuera para reutilizar la base
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true };
            using var sseClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
            sseClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

            // BUCLE MAESTRO: Si la conexión se cae, vuelve a intentar mientras no se cancele el token
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "Laboratorios/stream");
                    request.Headers.Add("Accept", "text/event-stream");

                    using var respuesta = await sseClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                    respuesta.EnsureSuccessStatusCode();

                    using var stream = await respuesta.Content.ReadAsStreamAsync(token);
                    using var reader = new StreamReader(stream);

                    while (!reader.EndOfStream && !token.IsCancellationRequested)
                    {
                        var linea = await reader.ReadLineAsync();

                        // Si la línea es nula, significa que el servidor cerró el stream. 
                        // Rompemos este ciclo para que el Bucle Maestro reinicie la conexión.
                        if (linea == null) break;

                        if (!string.IsNullOrWhiteSpace(linea) && linea.StartsWith("data:"))
                        {
                            var jsonPuro = linea.Substring(5).Trim();
                            var elementos = JsonSerializer.Deserialize<List<Laboratorios>>(jsonPuro, opcionesJson);

                            if (elementos != null && elementos.Count > 0)
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    alActualizar(elementos);
                                });
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // La cancelación se pidió desde el ViewModel (ej. cerraste la pantalla)
                    Console.WriteLine("Escucha de laboratorios detenida voluntariamente.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Conexión SSE interrumpida: {ex.Message}. Reconectando en 2 segundos...");
                    // Esperamos 2 segundos antes de reconectar para no bombardear tu Web API si se cae el server
                    await Task.Delay(2000, token);
                }
            }
        }
        public async Task<List<Prestamos>> ObtenerPrestamosPorUsuarioAsync(int usuarioId)
        {
            try
            {
                string url = $"Prestamos/usuario/{usuarioId}";
                using var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new List<Prestamos>(); // Retorna lista vacía si no hay registros
                }

                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<List<Prestamos>>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerPrestamosPorUsuarioAsync: {ex.Message}");
                throw;
            }
        }

        
        public async Task EscucharPrestamosSSEAsync(int usuarioId, Action<List<Prestamos>> onDataReceived, CancellationToken cancellationToken)
        {
                string url = $"Prestamos/usuario/{usuarioId}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);

                while (!cancellationToken.IsCancellationRequested && !reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();

                    if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("data: "))
                    {
                        string json = line.Substring("data: ".Length);
                        var listaPrestamos = JsonSerializer.Deserialize<List<Prestamos>>(json, _jsonOptions);

                        if (listaPrestamos != null)
                        {
                            onDataReceived?.Invoke(listaPrestamos);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Tarea cancelada por el ciclo de vida de la página. Comportamiento normal.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en canal SSE Préstamos: {ex.Message}");
            }
        }

    }
}