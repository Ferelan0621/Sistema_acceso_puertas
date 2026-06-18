namespace Movil.Pages;

public partial class Logout : ContentPage
{
	public Logout()
	{
		InitializeComponent();
	}

    private async void Cerrar_Sesión_Clicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new NavigationPage(new IniciarSesion());
    }
}