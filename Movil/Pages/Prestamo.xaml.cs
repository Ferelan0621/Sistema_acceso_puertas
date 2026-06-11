using Movil.Data;
using Movil.Services;
using Shared.Models;
using Shared.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;


namespace Movil.Pages;

public partial class Prestamo : ContentPage
{
    private ConexionMqtt miBroker;
    private readonly ApiService _apiService = new ApiService();
    public int userId { get; set; }


    public int UsuarioId { get; set; }

    public Prestamo()
    {
        InitializeComponent();
        miBroker = new ConexionMqtt();

        //Asignar el manejador de mensajes antes de conectar
        ConectarYSuscribir();
        //CargarDatosEnElPicker();
        userId = Preferences.Default.Get("usuarioID", 0);


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
            // 1. Usamos tu método exacto de la foto 3
            var laboratorios = await _apiService.ObtenerLaboratoriosAsync();

            // 2. Le pasamos los datos a la pantalla forzando el hilo principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (laboratorios != null && laboratorios.Count > 0)
            {
                // Llenamos el Picker
                pickerLab.ItemsSource = laboratorios;
                pickerLab.Title = "Selecciona un laboratorio...";
            }
            else
            {
                // Si la lista llegó vacía, avisamos
                pickerLab.Title = "No se encontraron laboratorios";
            }
            });
        }
        catch (Exception ex)
        {
            // Si algo truena, mostramos la alerta en el hilo principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                pickerLab.Title = "Error de conexión";
                DisplayAlert("Aviso", $"Ocurrió un error: {ex.Message}", "OK");
            });
        }
    }
    private async void prestamo_Clicked(object sender, EventArgs e)
    {
        if (pickerLab.SelectedIndex == -1)
        {
            DisplayAlert("Aviso", "Por favor, selecciona un laboratorio primero.", "OK");
            return; // Rompemos la ejecución para que no truene lo de abajo
        }

        // ==========================================================
        // 2. EXTRACCIÓN DEL OBJETO DEL PICKER (EL TRUCO)
        // ==========================================================
        // Sabemos que le metiste una lista de 'LaboratorioModel' al pickerLab.
        // Por lo tanto, el SelectedItem ES un 'LaboratorioModel'. Lo transformamos (casteo).

        Laboratorios labSeleccionado = (Laboratorios)pickerLab.SelectedItem;

        // ¡Boom! Ya tienes las variables separadas a tu disposición
        int idDelLaboratorio = labSeleccionado.ID;
        string nombreDelLaboratorio = labSeleccionado.nombreLaboratorio;


        // ==========================================================
        // 3. CAPTURA DE FECHA ACTUAL Y TIMEPICKER
        // ==========================================================
        // Capturamos el momento exacto en que presionó el botón (ideal para bitácoras)
        string fechaSolicitud = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

        // Extraemos la hora que escogió en el control
        string horastart = timeStart.Time.ToString();
        string horafinish = timeFinish.Time.ToString();


        // ==========================================================
        // 4. CREACIÓN DEL OBJETO PARA EL JSON
        // ==========================================================
        // Armamos un objeto anónimo con la estructura exacta que tu API 
        // o tu sistema de mensajería espera recibir.
        var payloadSolicitud = new
        {
            UsuarioID = userId,
            LaboratorioID = idDelLaboratorio,
            FechaPrestamo = fechaSolicitud,
            HoraInicio = horastart,
            HoraFinal = horafinish,
            estatus = "Pendiente" // Puedes meter datos estáticos de control
        };

        // ==========================================================
        // 5. SERIALIZACIÓN A JSON
        // ==========================================================
        var opcionesJson = new JsonSerializerOptions
        {
            WriteIndented = true, // Para que se vea bonito y estructurado
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Estandariza minúsculas al inicio
        };

        // Convertimos el objeto 'payloadSolicitud' en una cadena de texto JSON
        string jsonFinal = JsonSerializer.Serialize(payloadSolicitud, opcionesJson);
        LabelResultado.Text = jsonFinal;

        // Validamos que el elemento principal (el broker) no falte antes de intentar enviar
        if (miBroker == null)
        {
            DisplayAlertAsync("Falta inicializar el cliente MQTT. No se puede enviar la petición.", "Elemento Faltante", "OK", "Warning");
            return;
        }

        try
        {
            // Publicamos el mensaje "2" en el tópico definido en MqttServices.abrir ("UPT/LABORATORIOS")
            await miBroker.PublicarMensajeAsync(MqttServices.conexion, jsonFinal);

            // Confirmación visual opcional
            if (DisplayAlertAsync("La petición de apertura se envió correctamente.", "Éxito", "OK", "Information").IsCanceled)
            {

     
            }
        }
        catch (Exception ex)
        {
            // Capturamos cualquier error en caso de que falte conexión de red o falle el envío
            DisplayAlertAsync("Falta conexión o hubo un error al enviar el mensaje: " + ex.Message, "Error de Envío", "OK", "Error");
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
            var resultado = DisplayAlertAsync(
                "Error al inicializar la conexión MQTT: " + ex.Message,
                "Error de Conexión",
                "ok", "intentar");
            if (resultado.IsCanceled)
            {
                ConectarYSuscribir();
            }

        }
    }



    private async void Mensaje()
    {



        // Validamos que el elemento principal (el broker) no falte antes de intentar enviar
        if (miBroker == null)
        {
            DisplayAlertAsync("Falta inicializar el cliente MQTT. No se puede enviar la petición.", "Elemento Faltante", "OK", "Warning");
            return;
        }

        try
        {
            // Publicamos el mensaje "2" en el tópico definido en MqttServices.abrir ("UPT/LABORATORIOS")
            await miBroker.PublicarMensajeAsync(MqttServices.conexion, "abrir");

            // Confirmación visual opcional
            if (DisplayAlertAsync("La petición de apertura se envió correctamente.", "Éxito", "OK", "Information").IsCanceled)
            {

                //    Prueba.IsEnabled = false;
                //    await Task.Delay(5000);
                //    Prueba.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            // Capturamos cualquier error en caso de que falte conexión de red o falle el envío
            DisplayAlertAsync("Falta conexión o hubo un error al enviar el mensaje: " + ex.Message, "Error de Envío", "OK", "Error");
        }
    }

}