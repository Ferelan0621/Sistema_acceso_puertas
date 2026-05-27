namespace Movil.Pages;

public partial class Logout : ContentPage
{
	public Logout()
	{
		InitializeComponent();
	}

    private async void Cerrar_Sesión_Clicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync("//IniciarSesion");
    }
}