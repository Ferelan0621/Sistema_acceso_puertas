using Movil.Data;
using Movil.Services;
using Shared.Models;
using Shared.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;


namespace Movil.Pages;

public partial class Prestamo : ContentPage, IQueryAttributable

{
    private ConexionMqtt miBroker;
    private readonly ApiService _apiService = new ApiService();
    public int userId { get; set; }
    private Laboratorios _laboratorioActual;


    public Prestamo()
    {
        InitializeComponent();

       
    
        miBroker = new ConexionMqtt();

        //Asignar el manejador de mensajes antes de conectar
        ConectarYSuscribir();
        //CargarDatosEnElPicker();
        userId = Preferences.Default.Get("usuarioID", 0);


    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("LaboratorioClave", out var laboratorioObjeto))
        {
            // Guardamos el objeto en la variable global para que el botón lo pueda usar
            _laboratorioActual = (Laboratorios)laboratorioObjeto;
        }
    }

    // 4. TU BOTÓN DE PRÉSTAMO
    private async void prestamo_Clicked(object sender, EventArgs e)
    {
        // ¡Aquí ya no te debe marcar error porque la variable global ya existe y tiene datos!
        int idParaElJson = _laboratorioActual.ID;

        string fechaSolicitud = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

        string horastart = timeStart.Time.ToString();
        string horafinish = timeFinish.Time.ToString();

        var payloadSolicitud = new
        {
            UsuarioID = userId,
            LaboratorioID = idParaElJson,

            FechaPrestamo = fechaSolicitud
            
        };

        var opcionesJson = new JsonSerializerOptions
        {
            WriteIndented = true, 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        };

        string jsonFinal = JsonSerializer.Serialize(payloadSolicitud, opcionesJson);
        LabelResultado.Text = jsonFinal;

        if (miBroker == null)
        {
            DisplayAlertAsync("Falta inicializar el cliente MQTT. No se puede enviar la petición.", "Elemento Faltante", "OK", "Warning");
            return;
        }

        try
        {
            await miBroker.PublicarMensajeAsync(MqttServices.conexion, jsonFinal);

            if (DisplayAlertAsync("La petición de apertura se envió correctamente.", "Éxito", "OK", "Information").IsCanceled)
            {

     
            }
        }
        catch (Exception ex)
        {
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



   

}