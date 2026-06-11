using Movil.Data;
using Shared.Models;
using Shared.Services;
using System.Text.Json;
using Microsoft.Maui.Storage; 

namespace Movil.Pages;

public partial class Inicio : ContentPage
{
    private ConexionMqtt miBroker;
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



    private async void Prueba_Clicked(object sender, EventArgs e)
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

                Prueba.IsEnabled = false;
                await Task.Delay(5000);
                Prueba.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            // Capturamos cualquier error en caso de que falte conexión de red o falle el envío
            DisplayAlertAsync("Falta conexión o hubo un error al enviar el mensaje: " + ex.Message, "Error de Envío", "OK", "Error");
        }
    }   
}