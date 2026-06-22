using Movil.ViewModels;

namespace Movil.Pages;

public partial class Prestamo: ContentPage
{
    public Prestamo(PrestamosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PrestamosViewModel vm)
        {
            vm.IniciarEscuchaSSE();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is PrestamosViewModel vm)
        {
            vm.DetenerEscuchaSSE();
        }
    }
}