using Movil.Data;
using Shared.Models;
using System.Text.Json;
using Shared.Services;

namespace Movil.Pages;

public partial class IniciarSesion : ContentPage
{   

    

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
        await Shell.Current.GoToAsync("//mainapp");

    }

    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {

        await Shell.Current.GoToAsync("Password");
    }
}