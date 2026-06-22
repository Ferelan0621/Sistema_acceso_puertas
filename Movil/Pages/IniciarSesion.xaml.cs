using Movil.Data;
using Shared.Models;
using System.Text.Json;
using Shared.Services;
using Movil.Services;

namespace Movil.Pages;

public partial class IniciarSesion : ContentPage
{
    private readonly ApiService _apiService;


    public IniciarSesion()
    {

        InitializeComponent();
        _apiService = new ApiService();

        string nombreDelUsuario = Preferences.Default.Get("nombreUser", "Usuario Invitado");

        // Para obtener el ID:
        int idUsuario = Preferences.Default.Get("usuarioID", 0);
    }


  

    private async void btnIniciarsesion_Clicked(object sender, EventArgs e)
    {
      
        string clave = etClave.Text?.Trim();
        string password = etPasword.Text?.Trim();

        // 2. Validaciones básicas antes de gastar datos/red
        if (string.IsNullOrEmpty(clave) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Campos Vacíos", "Por favor, escribe tu Clave y Contraseña.", "OK");
            return;
        }

        try
        {
            // 3. Bloqueamos el botón y encendemos el spinner de carga
            btnIniciarsesion.IsEnabled = false;
            LoadingIndicator.IsRunning = true;

            // 4. Llamamos a nuestro servicio que hace el POST al Dev Tunnel
            bool loginExitoso = await _apiService.IniciarSesionAsync(clave, password);

            if (loginExitoso)
            {

                // ¡Éxito! Aquí lo mandas a la pantalla principal de tu app
                await Shell.Current.GoToAsync("//inicio");
               
            }
            else
            {
                // El servidor regresó 401 Unauthorized o falló la validación
                await DisplayAlert("Error", "Clave ISSEMYM o contraseña incorrectas.", "OK");
            }
        }
        catch (Exception ex)
        {
            // Por si el servidor está apagado o el Dev Tunnel expiró
            await DisplayAlert("Error de Conexión", $"No se pudo conectar al servidor: {ex.Message}", "OK");
        }
        finally
        {
            // 5. Pase lo que pase, volvemos a activar el botón y apagamos el spinner
            LoadingIndicator.IsRunning = false;
            btnIniciarsesion.IsEnabled = true;
        }
    }




    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {

        await Shell.Current.GoToAsync("Password");
    }
}