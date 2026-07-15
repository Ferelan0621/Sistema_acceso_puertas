using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Movil.Data;
using Movil.Services;
using Shared.Models;
using Shared.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Movil.ViewModels;

[QueryProperty(nameof(LaboratorioActual), "LaboratorioClave")]
public partial class InicioViewModel : ObservableObject
{
    private CancellationTokenSource _sseCts;
    private readonly ApiService _apiService = new ApiService();
    private readonly ConexionMqtt _miBroker;

    [ObservableProperty]
    private ObservableCollection<Laboratorios> _laboratorios = new();

    [ObservableProperty]
    private string _titulo = "Cargando...";

    [ObservableProperty]
    private string _resultadoJson;

    [ObservableProperty]
    private Laboratorios _laboratorioActual;

    [ObservableProperty]
    private Laboratorios _laboratorioSeleccionado;

    // Propiedad clave para saber cuál nos aceptaron (-1 significa ninguno)
    [ObservableProperty]
    private int _laboratorioAceptadoId = -1;

    // Clase interna para desarmar el JSON de MQTT exactamente como lo envías
    private class RespuestaMqtt
    {
        public string estatus { get; set; }
        public int usuarioID { get; set; }
        public int laboratorioID { get; set; }
        public string mensaje { get; set; }
    }

    public InicioViewModel()
    {
        _miBroker = new ConexionMqtt();
        CargarEstadoUsuario(); // Recupera el estado guardado al iniciar la vista
        _ = IniciarComunicacionMqtt();
    }

    private int userId => Preferences.Default.Get("usuarioID", 0);
    public string username => Preferences.Default.Get("Nombre", "usuariodef");

    // =====================================================================
    // NUEVA LÓGICA DE SESIÓN E INACTIVIDAD (Separada por Usuario)
    // =====================================================================
    private void CargarEstadoUsuario()
    {
        if (userId != 0)
        {
            // Carga específicamente el laboratorio que este usuario tenía prestado
            LaboratorioAceptadoId = Preferences.Default.Get($"LabAceptado_{userId}", -1);
        }
    }

    public void ValidarInactividad()
    {
        if (userId == 0) return;

        var ultimaActividad = Preferences.Default.Get($"UltimaActividad_{userId}", DateTime.Now);
        var tiempoInactivo = DateTime.Now - ultimaActividad;

        if (tiempoInactivo.TotalHours >= 5)
        {
            // Forzar cierre por inactividad
            Preferences.Default.Remove("usuarioID");
            Preferences.Default.Remove("Nombre");

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.DisplayAlert("Sesión Expirada", "Tu sesión se ha cerrado por inactividad.", "OK");
                await Shell.Current.GoToAsync("//IniciarSesion"); // Verifica que tu ruta se llame así
            });
        }
        else
        {
            RegistrarActividad();
        }
    }

    public void RegistrarActividad()
    {
        if (userId != 0)
        {
            Preferences.Default.Set($"UltimaActividad_{userId}", DateTime.Now);
        }
    }
    // =====================================================================

    public void IniciarEscuchaSSE()
    {
        _sseCts = new CancellationTokenSource();
        _ = _apiService.EscucharActualizacionesSSEAsync(
            datosNuevos => ActualizarListaUI(datosNuevos),
            _sseCts.Token
        );
    }

    [RelayCommand]
    public void LiberarLaboratorio()
    {
        LaboratorioAceptadoId = -1;
        Preferences.Default.Set($"LabAceptado_{userId}", -1); // Persistir liberación
        RegistrarActividad();
        _ = CargarLaboratoriosAsync();
    }

    public void DetenerEscuchaSSE()
    {
        _sseCts?.Cancel();
        _sseCts?.Dispose();
    }

    private void ActualizarListaUI(List<Laboratorios> nuevosDatos)
    {
        Laboratorios.Clear();

        // 1. Si ya tenemos un laboratorio aceptado, lo ponemos HASTA ARRIBA
        if (LaboratorioAceptadoId != -1)
        {
            var labAceptado = nuevosDatos.FirstOrDefault(l => l.ID == LaboratorioAceptadoId);
            if (labAceptado != null)
            {
                Laboratorios.Add(labAceptado);
                nuevosDatos.Remove(labAceptado);
            }
        }

        // 2. Agregamos el resto abajo
        foreach (var lab in nuevosDatos)
        {
            Laboratorios.Add(lab);
        }

        Titulo = "Selecciona un Lab";
    }

    [RelayCommand]
    private async Task CargarLaboratoriosAsync()
    {
        try
        {
            var listaLabs = await _apiService.ObtenerLaboratoriosAsync();
            if (listaLabs != null)
            {
                ActualizarListaUI(listaLabs);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task SeleccionarLaboratorioAsync(Laboratorios labSeleccionado)
    {
        RegistrarActividad(); // Refrescar el contador de 5 horas

        if (labSeleccionado == null) return;

        // Limpiamos la selección visual para evitar que se quede "pegado"
        LaboratorioSeleccionado = null;

        // CASO A: YA TENEMOS UN LABORATORIO ACEPTADO
        if (LaboratorioAceptadoId != -1)
        {
            if (labSeleccionado.ID != LaboratorioAceptadoId)
            {
                // Le dio clic a uno gris/inhabilitado
                await Shell.Current.DisplayAlertAsync("Aviso", "Ya tienes un laboratorio en uso. Los demás están inhabilitados.", "OK");
                return;
            }
            else
            {
                // ES EL LABORATORIO ACEPTADO: Mandar comando secundario (ej. cerrar puerta o liberar)
                string fechaCierre = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

                var payloadSecundario = new
                {
                    UsuarioID = userId,
                    LaboratorioID = labSeleccionado.ID,
                    FechaCierreRemoto = fechaCierre
                };

                string jsonSecundario = JsonSerializer.Serialize(payloadSecundario, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                try
                {
                    // Corregido: Faltaba el punto y coma al final de esta línea
                    bool cerrar = await Shell.Current.DisplayAlertAsync("Cerrar Laboratorio", "¿Deseas cerrar el laboratorio de manera remota?", "Si", "No");
                    if (cerrar)
                    {
                        await _miBroker.PublicarMensajeAsync(MqttServices.cerrado, jsonSecundario);
                        await Shell.Current.DisplayAlert("Cerrando Laboratorio", "Espere confirmacion de cierre.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Fallo al enviar: {ex.Message}", "OK");
                }
                return;
            }
        }

        // CASO B: FLUJO NORMAL (Pedir préstamo de uno nuevo)
        if (labSeleccionado.Estatus != EstadoLaboratorio.Disponible)
        {
            await Shell.Current.DisplayAlertAsync("Aviso", $"El Laboratorio {labSeleccionado.NombreLaboratorio} no está disponible.", "OK");
            return;
        }

        string fechaSolicitud = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        var payloadSolicitud = new
        {
            UsuarioID = userId,
            LaboratorioID = labSeleccionado.ID,
            FechaPrestamo = fechaSolicitud
        };

        var opcionesJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        string jsonFinal = JsonSerializer.Serialize(payloadSolicitud, opcionesJson);
        ResultadoJson = jsonFinal;

        if (_miBroker == null)
        {
            await Shell.Current.DisplayAlert("Elemento Faltante", "Falta inicializar el cliente MQTT.", "OK");
            return;
        }

        try
        {
            await _miBroker.PublicarMensajeAsync(MqttServices.conexion, jsonFinal);
            await Shell.Current.DisplayAlert("Éxito", "Petición de apertura enviada.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Error al enviar el mensaje: {ex.Message}", "OK");
        }
    }

    private async Task IniciarComunicacionMqtt()
    {
        if (userId == 0) return;
        try
        {
            _miBroker.MensajeRecibido += AlRecibirMensajeMqtt;
            await _miBroker.ConectarAsync();
            await _miBroker.SuscribirseAsync($"{MqttServices.respuesta}/{userId}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error De Conexion", $"Error al conectar MQTT: {ex.Message}", "Ok");
        }
    }

    private void AlRecibirMensajeMqtt(string topic, string payload)
    {
        if (topic == $"{MqttServices.respuesta}/{userId}")
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Convertimos el JSON entrante
                    var respuesta = JsonSerializer.Deserialize<RespuestaMqtt>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (respuesta != null && respuesta.estatus.Equals("aceptado", StringComparison.OrdinalIgnoreCase))
                    {
                        // NOS ACEPTARON: Asignamos el ID y lo guardamos
                        LaboratorioAceptadoId = respuesta.laboratorioID;
                        Preferences.Default.Set($"LabAceptado_{userId}", LaboratorioAceptadoId);

                        // Reordenar la lista para forzar que quede en primer plano
                        var labAceptado = Laboratorios.FirstOrDefault(l => l.ID == LaboratorioAceptadoId);
                        if (labAceptado != null)
                        {
                            Laboratorios.Remove(labAceptado);
                            Laboratorios.Insert(0, labAceptado);
                        }

                        await Shell.Current.DisplayAlert("¡Acceso Concedido!", respuesta.mensaje, "OK");
                    }
                    else if (respuesta != null && respuesta.estatus.Equals("denegado", StringComparison.OrdinalIgnoreCase))
                    {
                        // NOS DENEGARON
                        LaboratorioAceptadoId = -1;
                        await Shell.Current.DisplayAlert("Acceso Denegado", respuesta.mensaje, "OK");
                    }
                    else if (respuesta != null && respuesta.estatus.Equals("cerrado", StringComparison.OrdinalIgnoreCase))
                    {
                        // SE CERRÓ: Limpiar estado y disparar alerta de segundo plano
                        LaboratorioAceptadoId = -1;
                        Preferences.Default.Set($"LabAceptado_{userId}", -1);


                        await Shell.Current.DisplayAlert("Laboratorio Cerrado", respuesta.mensaje, "OK");
                        LiberarLaboratorio();
                    }
                }
                catch
                {
                    await Shell.Current.DisplayAlert("Mensaje", payload, "OK");
                }
            });
        }
    }

    public void LimpiarRecursos()
    {
        if (_miBroker != null)
        {
            _miBroker.MensajeRecibido -= AlRecibirMensajeMqtt;
        }

        Laboratorios?.Clear();
        ResultadoJson = string.Empty;
        LaboratorioSeleccionado = null;
        if (userId != userId)
        {
            LiberarLaboratorio();
        }
    }
}