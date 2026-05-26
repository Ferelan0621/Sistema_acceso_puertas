namespace Movil;

public partial class MyShell : Shell
{
	public MyShell()
	{
		InitializeComponent();
		Rutas();
	}
	public async void Rutas()
	{
		await Shell.Current.GoToAsync("InicioSesion");
		await Shell.Current.GoToAsync("Registro");
		await Shell.Current.GoToAsync("Password");
		await Shell.Current.GoToAsync("mainapp");

	}
}