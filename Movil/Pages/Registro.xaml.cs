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
   
    private void Registrar_Clicked(object sender, EventArgs e)
    {

    }
}