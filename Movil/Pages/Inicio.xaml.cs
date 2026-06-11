using Microsoft.Maui.Storage; 
using Movil.Data;
using Movil.Services;
using Shared.Models;
using Shared.Services;
using System.Text.Json;

namespace Movil.Pages;

public partial class Inicio : ContentPage
{
    private ConexionMqtt miBroker;
    private readonly ApiService _apiService = new ApiService();

    public int NombreUsuario { get; set; }
    public Inicio()
    {
        InitializeComponent();
        miBroker = new ConexionMqtt();

        //Asignar el manejador de mensajes antes de conectar
        
        ConectarYSuscribir();
        NombreUsuario = Preferences.Default.Get("usuarioID", 0);

        // 2. Le decimos a la página que ella misma es su fuente de datos
        this.BindingContext = this;

    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarLaboratorios();
    }
    private async void CargarLaboratorios()
    {
        try
        {
            var listaLabs = await _apiService.ObtenerLaboratoriosAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (listaLabs != null && listaLabs.Count > 0)
                {
                    // Pasamos la lista al CollectionView que ahora es un Grid
                    gridLaboratorios.ItemsSource = listaLabs;
                    lblTitulo.Text = "Selecciona un Lab";
                }
                else
                {
                    lblTitulo.Text = "No hay laboratorios";
                }
            });
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                DisplayAlert("Error", ex.Message, "OK");
            });
        }
    }


    private async void ConectarYSuscribir()
    {
        try
        {
            await miBroker.ConectarAsync();
            await miBroker.SuscribirseAsync(MqttServices.statusTopic);
        }
        catch (Exception ex)
        {
            // Mostramos el mensaje y capturamos el resultado del botón presionado
            var resultado =  DisplayAlertAsync(
                "Error al inicializar la conexión MQTT: " + ex.Message,
                "Error de Conexión",
                "ok", "intentar");
            if (resultado.IsCanceled)
            {
                ConectarYSuscribir();
            }
            
        }
    }

    private async void OnLaboratorioSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        // 1. Validar que el usuario realmente haya tocado una tarjeta
        if (e.CurrentSelection.FirstOrDefault() == null)
            return;

        // 2. Extraemos tu objeto con la clase exacta
        Laboratorios labSeleccionado = (Laboratorios)e.CurrentSelection.FirstOrDefault();

        // 3. Limpiamos la selección del Grid inmediatamente para evitar bloqueos al regresar
        gridLaboratorios.SelectedItem = null;

        // 4. Creamos el diccionario de parámetros de navegación de Shell
        // "LaboratorioClave" es el apodo con el que viajará el objeto en la mochila
        var parametrosNavegacion = new Dictionary<string, object>
    {
        { "LaboratorioClave", labSeleccionado }
    };

        // 5. ¡VÁMONOS! Navegamos usando la ruta que registramos en el Paso 1
        await Shell.Current.GoToAsync("Prestamo", parametrosNavegacion);
    }

   
}