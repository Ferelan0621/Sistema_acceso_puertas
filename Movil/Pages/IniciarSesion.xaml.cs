using Movil.Data;
using Shared.Models;
using System.Text.Json;
using Shared.Services;

namespace Movil.Pages;

public partial class IniciarSesion : ContentPage
{   

    public ConexionMqtt _mqtt = new ConexionMqtt();


    ConexionMqtt conexion = new();

    public IniciarSesion()
	{
        
		InitializeComponent();
        conectarMqtt();


    }
    private async void conectarMqtt()
    {
        try
        {
            await conexion.ConectarAsync();
        }
        catch(Exception ex)
        {
            string texto = "no jalo padrino";
            txtUsuario.Text = texto;
        }
    }

    private void btnRegistrar_Clicked(object sender, EventArgs e)
    {
        JsonAbrir jsonla2 = new JsonAbrir
        { d = "3", c = "abrir" };

        string jsonString = JsonSerializer.Serialize(jsonla2);
        _mqtt.PublicarMensajeAsync(MqttServices.abrir, jsonString);
    }

    private void btnIniciarsesion_Clicked(object sender, EventArgs e)
    {

    }

    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        
        await Navigation.PushAsync(new Password());
    }
}