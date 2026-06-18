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
    private readonly ApiService _apiService = new ApiService();
    private readonly ConexionMqtt _miBroker;
    private readonly int _userId;

    // Propiedades observables automáticas gracias al Toolkit
    [ObservableProperty]
    private ObservableCollection<Laboratorios> _laboratorios = new();

    [ObservableProperty]
    private bool isRefreshing;


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
        _ = ConectarYSuscribirAsync();
    }
    [RelayCommand]
    public async Task RefrescarDatos()
    {
        // 1. Si ya está cargando, evitamos que se ejecute dos veces
        if (IsRefreshing) return;

        try
        {
            IsRefreshing = true; // Enciende la ruedita

            // 2. Aquí haces la petición a tu API o base de datos
            await CargarLaboratoriosAsync();
        }
        catch (Exception ex)
        {
            // Si algo falla, puedes mostrar una alerta aquí
            // Usar el signo de interrogación evita que truene si la pantalla no está lista
            await Application.Current?.MainPage?.DisplayAlert("no jalo we", "no jala mamon", "ok");
        }
        finally
        {
            // 3. El bloque 'finally' GARANTIZA que, pase lo que pase 
            // (con error o sin error), la ruedita se va a apagar.
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task CargarLaboratoriosAsync()
    {
        try
        {
            var listaLabs = await _apiService.ObtenerLaboratoriosAsync();

            if (listaLabs != null && listaLabs.Count > 0)
            {
                Laboratorios = new ObservableCollection<Laboratorios>(listaLabs);
                Titulo = "Selecciona un Lab";
            }
            else
            {
                Titulo = "No hay laboratorios";
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
            await _miBroker.SuscribirseAsync(MqttServices.statusTopic);
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
            await Shell.Current.DisplayAlert("Éxito", "La petición de apertura se envió correctamente.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error de Envío", $"Falta conexión o hubo un error al enviar el mensaje: {ex.Message}", "OK");
        }
    }
}