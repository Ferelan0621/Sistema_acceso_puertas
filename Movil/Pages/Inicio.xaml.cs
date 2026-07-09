using Movil.ViewModels;

namespace Movil.Pages;

public partial class Inicio : ContentPage
{
    private readonly InicioViewModel _viewModel;

    public Inicio()
    {
        InitializeComponent();

        _viewModel = new InicioViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.CargarLaboratoriosCommand.CanExecute(null))
        {
            _viewModel.CargarLaboratoriosCommand.Execute(null);
        }

        _viewModel.IniciarEscuchaSSE();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _viewModel.DetenerEscuchaSSE();
        _viewModel.LimpiarRecursos();
    }
}