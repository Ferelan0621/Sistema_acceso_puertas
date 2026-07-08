using Shared.Models;
using Shared.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace Movil.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = ConexionHTTP.BaseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<bool> RecuperarContrasenaAsync(string claveImssemym, string nuevaPassword)
    {
        var datosRecuperacion = new { ClaveISSEMYM = claveImssemym, NuevaPassword = nuevaPassword };
        var response = await _httpClient.PostAsJsonAsync("Usuarios/Recuperar", datosRecuperacion);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IniciarSesionAsync(string clave, string password)
    {
        try
        {
            var datosLogin = new { ClaveISSEMYM = clave, Password = password };
            var response = await _httpClient.PostAsJsonAsync("Usuarios/login", datosLogin);

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<RespuestaLogin>();
                if (resultado != null)
                {
                    Microsoft.Maui.Storage.Preferences.Default.Set("usuarioID", resultado.usuarioID);
                    Microsoft.Maui.Storage.Preferences.Default.Set("Nombre", resultado.nombreUser);
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de conexión: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Laboratorios>> ObtenerLaboratoriosAsync()
    {
        try
        {
            var respuesta = await _httpClient.GetAsync("Laboratorios");
            respuesta.EnsureSuccessStatusCode();

            var contenido = await respuesta.Content.ReadAsStringAsync();
            var elementos = JsonSerializer.Deserialize<List<Laboratorios>>(contenido, _jsonOptions);

            return elementos ?? new List<Laboratorios>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la API: {ex.Message}");
            return new List<Laboratorios>();
        }
    }

    public async Task EscucharActualizacionesSSEAsync(Action<List<Laboratorios>> alActualizar, CancellationToken token)
    {
        var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true };
        using var sseClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        sseClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

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
                    if (linea == null) break;

                    if (!string.IsNullOrWhiteSpace(linea) && linea.StartsWith("data:"))
                    {
                        var jsonPuro = linea.Substring(5).Trim();
                        var elementos = JsonSerializer.Deserialize<List<Laboratorios>>(jsonPuro, _jsonOptions);

                        if (elementos != null)
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
                break;
            }
            catch (Exception)
            {
                await Task.Delay(2000, token);
            }
        }
    }

    public async Task<List<Prestamos>> ObtenerPrestamosPorUsuariosAsync(int usuarioId)
    {
        try
        {
            var respuesta = await _httpClient.GetAsync($"Prestamos/usuario/{usuarioId}");
            respuesta.EnsureSuccessStatusCode();

            var contenido = await respuesta.Content.ReadAsStringAsync();
            var elementos = JsonSerializer.Deserialize<List<Prestamos>>(contenido, _jsonOptions);

            return elementos ?? new List<Prestamos>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la API: {ex.Message}");
            return new List<Prestamos>();
        }
    }

    public async Task EscucharPrestamosSSEAsync(int usuarioId, Action<List<Prestamos>> alActualizar, CancellationToken token)
    {
        var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true };
        using var sseClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        sseClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

        while (!token.IsCancellationRequested)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "Prestamos/stream");
                request.Headers.Add("Accept", "text/event-stream");

                using var respuesta = await sseClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                respuesta.EnsureSuccessStatusCode();

                using var stream = await respuesta.Content.ReadAsStreamAsync(token);
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream && !token.IsCancellationRequested)
                {
                    var linea = await reader.ReadLineAsync();
                    if (linea == null) break;

                    if (!string.IsNullOrWhiteSpace(linea) && linea.StartsWith("data:"))
                    {
                        var jsonPuro = linea.Substring(5).Trim();
                        var elementos = JsonSerializer.Deserialize<List<Prestamos>>(jsonPuro, _jsonOptions);

                        if (elementos != null)
                        {
                            // 🔑 LA MAGIA OCURRE AQUÍ: Filtramos la lista entrante del stream 
                            // para que el usuario solo reciba SUS préstamos, ordenados.
                            var misPrestamos = elementos
                                .Where(p => p.UsuarioID == usuarioId)
                                .OrderByDescending(p => p.FechaSolicitud)
                                .ToList();

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                alActualizar(misPrestamos);
                            });
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                await Task.Delay(2000, token);
            }
        }
    }
}