
namespace Movil.Pages;

public partial class Logout : ContentPage
{
	public Logout()
	{
		InitializeComponent();
	}

    private async void Cerrar_Sesión_Clicked(object sender, EventArgs e)
    {
       bool salir = await Shell.Current.DisplayAlert("Cerrar Sesión", "¿Estás seguro de que quieres cerrar sesión?", "Sí", "No");
        if (salir)
        {
            Preferences.Default.Remove("usuarioID");
            Preferences.Default.Remove("nombreUser");
            await Shell.Current.GoToAsync("//IniciarSesion");
        }
        
    }
}