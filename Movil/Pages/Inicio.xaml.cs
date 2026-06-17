using Movil.ViewModels;

namespace Movil.Pages;

public partial class Inicio : ContentPage
{
    private readonly InicioViewModel _viewModel;

    public Inicio()
    {
        InitializeComponent();

        // Asignamos el ViewModel como BindingContext
        _viewModel = new InicioViewModel();
        this.BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Ejecutamos el comando de carga usando el comando generado por el Toolkit
        if (_viewModel.CargarLaboratoriosCommand.CanExecute(null))
        {
            _viewModel.CargarLaboratoriosCommand.Execute(null);
        }
    }
}