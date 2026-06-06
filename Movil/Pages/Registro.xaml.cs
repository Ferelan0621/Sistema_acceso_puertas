using Movil.Services;

namespace Movil.Pages;

public partial class Registro : ContentPage
{
    private readonly ApiService _apiService;

    public Registro()
	{
		InitializeComponent();
        _apiService = new ApiService();
    }
    private async void CargarRoles()
    {
        // Asumiendo que en tu ApiService tienes un método que le pega al GET de roles
        // y te devuelve un List<RolDto>
        var roles = await _apiService.ObtenerRolesAsync();

        if (roles != null)
        {
            RolPicker.ItemsSource = roles;
        }
    }
    private void Registrar_Clicked(object sender, EventArgs e)
    {

    }
}