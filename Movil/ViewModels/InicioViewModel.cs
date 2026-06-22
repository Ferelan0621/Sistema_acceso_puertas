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

// Vinculamos la propiedad de navegación directamente al ViewModel
[QueryProperty(nameof(LaboratorioActual), "LaboratorioClave")]
public partial class InicioViewModel : ObservableObject
{

    private CancellationTokenSource _sseCts;

    private readonly ApiService _apiService = new ApiService();
    private readonly ConexionMqtt _miBroker;
    private readonly int _userId;

    // Propiedades observables automáticas gracias al Toolkit
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

    public InicioViewModel()
    {
        _miBroker = new ConexionMqtt();
        _userId = Preferences.Default.Get("usuarioID", 0);

        // Inicializamos la conexión MQTT en segundo plano sin bloquear el constructor
        _ = IniciarComunicacionMqtt();
    }
    


    public void IniciarEscuchaSSE()
    {
        _sseCts = new CancellationTokenSource();
        _ = _apiService.EscucharActualizacionesSSEAsync(
            datosNuevos => ActualizarListaUI(datosNuevos),
            _sseCts.Token
        );
    }

    public void DetenerEscuchaSSE()
    {
        _sseCts?.Cancel();
        _sseCts?.Dispose();
    }

    private void ActualizarListaUI(List<Laboratorios> nuevosDatos)
    {
        // Limpiamos y repoblamos la misma instancia de la colección.
        // Esto mantiene vivo el binding de tu vista.
        Laboratorios.Clear();

        foreach (var lab in nuevosDatos)
        {
            Laboratorios.Add(lab);
        }

        Titulo = "Selecciona un Lab";
    }


    // Asegúrate de actualizar tu método CargarLaboratoriosAsync para usar el nuevo estado
    [RelayCommand]
    private async Task CargarLaboratoriosAsync()
    {
        try
        {
            var listaLabs = await _apiService.ObtenerLaboratoriosAsync();
            if (listaLabs != null)
            {
                Laboratorios = new ObservableCollection<Laboratorios>(listaLabs);
                Titulo = "Selecciona un Lab";
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task ConectarYSuscribirAsync()
    {
        try
        {
            await _miBroker.ConectarAsync();
            await _miBroker.SuscribirseAsync(MqttServices.respuesta);
        }
        catch (Exception ex)
        {
            // Shell.Current.DisplayAlert devuelve true si presionan el primer botón ("Intentar de nuevo")
            bool reintentar = await Shell.Current.DisplayAlert(
                "Error de Conexión",
                $"Error al inicializar la conexión MQTT: {ex.Message}",
                "Intentar de nuevo",
                "OK");

            if (reintentar)
            {
                await ConectarYSuscribirAsync();
            }
        }
    }

    [RelayCommand]
    private async Task SeleccionarLaboratorioAsync(Laboratorios labSeleccionado)
    {
        if (labSeleccionado == null) return;

        // Limpiamos la selección para evitar bloqueos visuales
        LaboratorioSeleccionado = null;

        if (labSeleccionado.Estatus != EstadoLaboratorio.Disponible)
        {
            await Shell.Current.DisplayAlertAsync("Aviso", $"El Laboratorio {labSeleccionado.NombreLaboratorio} no esta disponible.", "OK");
            return;
        }

        string fechaSolicitud = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

        var payloadSolicitud = new
        {
            UsuarioID = _userId,
            LaboratorioID = labSeleccionado.ID,
            FechaPrestamo = fechaSolicitud
        };

        var opcionesJson = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string jsonFinal = JsonSerializer.Serialize(payloadSolicitud, opcionesJson);
        ResultadoJson = jsonFinal;

        if (_miBroker == null)
        {
            await Shell.Current.DisplayAlert("Elemento Faltante", "Falta inicializar el cliente MQTT. No se puede enviar la petición.", "OK");
            return;
        }

        try
        {
            await _miBroker.PublicarMensajeAsync(MqttServices.conexion, jsonFinal);
            await Shell.Current.DisplayAlert("Éxito", "La petici" +
                "ón de apertura se envió correctamente.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error de Envío", $"Falta conexión o hubo un error al enviar el mensaje: {ex.Message}", "OK");
        }
    }
    
    private async Task IniciarComunicacionMqtt()
    {
        try
        {
            // 1. Nos suscribimos al evento que creaste en EscritorioMQTT.cs
            _miBroker.MensajeRecibido += AlRecibirMensajeMqtt;

            // 2. Nos conectamos al broker
            await _miBroker.ConectarAsync();

            // 3. Nos suscribimos al tópico específico del móvil (el que estaba en azul)
            await _miBroker.SuscribirseAsync(MqttServices.respuesta);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error De Conexion", $"Error al conectar MQTT: {ex.Message}", "Ok");
        }
    }

    // Este método se dispara gracias al "MensajeRecibido?.Invoke(topic, payload);" de tu clase
    private void AlRecibirMensajeMqtt(string topic, string payload)
    {
        if (topic == MqttServices.respuesta)
        {
            // AHORA SÍ pasamos al hilo principal
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.DisplayAlert("Mensaje", payload, "OK");
                // Aquí ya puedes actualizar tu Canvas tranquilamente
            });
        }
    }
}