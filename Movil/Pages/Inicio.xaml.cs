using Movil.ViewModels;

namespace Movil.Pages;

public partial class Inicio : ContentPage
{
    private readonly InicioViewModel _viewModel;

    public Inicio()
    {
        InitializeComponent();

        // Asignamos el ViewModel
        _viewModel = new InicioViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 1. Cargamos la lista inicial
        if (_viewModel.CargarLaboratoriosCommand.CanExecute(null))
        {
            _viewModel.CargarLaboratoriosCommand.Execute(null);
        }

        // 2. Iniciamos la conexión SSE para recibir actualizaciones en tiempo real
        _viewModel.IniciarEscuchaSSE();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // 3. ¡IMPORTANTE! Cerramos la conexión SSE al salir para liberar recursos
        _viewModel.DetenerEscuchaSSE();
    }
}