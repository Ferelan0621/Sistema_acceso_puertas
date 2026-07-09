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
	public class ApiService
	{
		private readonly HttpClient _httpClient;
		private const string BaseUrl = ConexionHTTP.BaseUrl;

		public ApiService()
		{
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
				var datosLogin = new
				{
					Nombre = usuario,
					Password = password
				};

				var response = await _httpClient.PostAsJsonAsync("Encargados/login", datosLogin);

				if (response.IsSuccessStatusCode)
				{
					return true;
				}
				else
				{
					string errorContent = await response.Content.ReadAsStringAsync();
					System.Diagnostics.Debug.WriteLine($"Error del servidor: {errorContent}");
					return false;
				}
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
				Console.WriteLine($"Error en la API: {ex.Message}");
				return new List<Laboratorios>();
			}
		}

		public async Task<Usuarios> ObtenerUsuarioPorIdAsync(int id)
		{
			try
			{
				var opcionesJson = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};

				var respuesta = await _httpClient.GetAsync($"Usuarios/{id}");

				if (!respuesta.IsSuccessStatusCode)
				{
					System.Diagnostics.Debug.WriteLine($"[API] Usuario {id} no encontrado. Status: {respuesta.StatusCode}");
					return null;
				}

				var contenido = await respuesta.Content.ReadAsStringAsync();
				var usuario = JsonSerializer.Deserialize<Usuarios>(contenido, opcionesJson);
				return usuario;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[API] Error obteniendo usuario {id}: {ex.Message}");
				return null;
			}
		}

		// 🔑 NUEVO: Guarda un nuevo préstamo en BD
		public async Task<Prestamos> GuardarPrestamoAsync(Prestamos prestamo)
		{
			try
			{
				var respuesta = await _httpClient.PostAsJsonAsync("Prestamos", prestamo);
				respuesta.EnsureSuccessStatusCode();

				var contenido = await respuesta.Content.ReadAsStringAsync();
				var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				var guardado = JsonSerializer.Deserialize<Prestamos>(contenido, opcionesJson);

				System.Diagnostics.Debug.WriteLine($"[API] Préstamo guardado con ID: {guardado?.ID}");
				return guardado;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[API] Error guardando préstamo: {ex.Message}");
				return null;
			}
		}

		public async Task<bool> ActualizarLaboratorioAsync(Laboratorios laboratorio)
		{
			try
			{
				// Agregar debug para ver qué se manda
				string jsonDebug = JsonSerializer.Serialize(laboratorio);
				System.Diagnostics.Debug.WriteLine($"[API] PUT Laboratorios/{laboratorio.ID} → {jsonDebug}");

				var respuesta = await _httpClient.PutAsJsonAsync("Laboratorios/" + laboratorio.ID, laboratorio);

				System.Diagnostics.Debug.WriteLine($"[API] Respuesta: {respuesta.StatusCode}");

				respuesta.EnsureSuccessStatusCode();
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[API] Error: {ex.Message}");
				return false;
			}
		}

		// 🔑 NUEVO: Actualiza la fecha de cierre de un préstamo
		public async Task<bool> CerrarPrestamoAsync(int id, DateTime fechaCierre)
		{
			try
			{
				// Primero obtenemos el préstamo actual
				var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				var respuestaGet = await _httpClient.GetAsync($"Prestamos/{id}");
				respuestaGet.EnsureSuccessStatusCode();

				var contenido = await respuestaGet.Content.ReadAsStringAsync();
				var prestamo = JsonSerializer.Deserialize<Prestamos>(contenido, opcionesJson);

				if (prestamo == null) return false;

				// Actualizamos las fechas de cierre
				prestamo.FechaCierre = fechaCierre;
				prestamo.FechaCierreRemoto = fechaCierre;

				var respuestaPut = await _httpClient.PutAsJsonAsync($"Prestamos/{id}", prestamo);
				respuestaPut.EnsureSuccessStatusCode();

				System.Diagnostics.Debug.WriteLine($"[API] Préstamo {id} cerrado correctamente");
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[API] Error cerrando préstamo: {ex.Message}");
				return false;
			}
		}

		public async Task EscucharPuertasSSEAsync(Action<SensorPuerta> onMensajeRecibido, CancellationToken cancellationToken)
		{
			try
			{
				using var request = new HttpRequestMessage(HttpMethod.Get, "Laboratorios/stream-puertas");
				request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

				using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
				response.EnsureSuccessStatusCode();

				using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
				using var reader = new StreamReader(stream);

				while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
				{
					var line = await reader.ReadLineAsync();

					if (string.IsNullOrWhiteSpace(line)) continue;

					if (line.StartsWith("data: "))
					{
						var json = line.Substring(6);

						try
						{
							var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
							var evento = JsonSerializer.Deserialize<SensorPuerta>(json, opcionesJson);

							if (evento != null)
							{
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
			}
		}

		public async Task<List<Prestamos>> ObtenerHistorialPrestamosAsync()
		{
			try
			{
				var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

				var respuesta = await _httpClient.GetAsync("Prestamos");
				respuesta.EnsureSuccessStatusCode();

				var contenido = await respuesta.Content.ReadAsStringAsync();
				var elementos = JsonSerializer.Deserialize<List<Prestamos>>(contenido, opcionesJson);

				return elementos ?? new List<Prestamos>();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error en la API cargando historial: {ex.Message}");
				return new List<Prestamos>();
			}

		}
	}
}