using Movil.Data;
using Shared.Models;
using System.Text.Json;
using Shared.Services;
using Movil.Services;

namespace Movil.Pages;

public partial class IniciarSesion : ContentPage
{
    private readonly ApiService _apiService = new ApiService();


    public IniciarSesion()
    {

        InitializeComponent();



    }


    private async void btnRegistrar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Registro");

    }

    private async void btnIniciarsesion_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(etUsuario.Text) || string.IsNullOrWhiteSpace(etPasword.Text))
        {
            await DisplayAlert("Error", "Por favor, llena todos los campos.", "OK");
            return;
        }

        // 2. Llamar al endpoint a través de tu ApiService
        Usuarios usuarioLogueado = await _apiService.LoginAsync(etUsuario.Text, etPasword.Text);

        // 3. Validar la respuesta del API
        if (usuarioLogueado != null)
        {
            await DisplayAlert("¡Bienvenido!", $"Hola {usuarioLogueado.Nombre}", "OK");

            // Aquí puedes navegar a tu pantalla principal (ejemplo)
             await Shell.Current.GoToAsync("//mainapp0");
        }
        else
        {
            await DisplayAlert("Error de acceso", "Clave IMSSEMYM o contraseña incorrectas.", "OK");
        }
    }




    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {

        await Shell.Current.GoToAsync("Password");
    }
}