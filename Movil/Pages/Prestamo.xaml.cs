using Movil.ViewModels;

namespace Movil.Pages;

public partial class Prestamo : ContentPage
{
    private readonly PrestamosViewModel _viewModel;

    public Prestamo()
    {
        InitializeComponent();

        // Asignamos el ViewModel
        _viewModel = new PrestamosViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 1. Cargamos la lista inicial
        if (_viewModel.CargarPrestamosCommand.CanExecute(null))
        {
            _viewModel.CargarPrestamosCommand.Execute(null);
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