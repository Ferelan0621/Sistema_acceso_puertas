using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Pausa estratégica inicial: permite al Shell completar la transición visual de pestañas al 100%
        await Task.Delay(150);

        // 2. Ejecución asíncrona del comando de carga de laboratorios
        if (_viewModel.CargarLaboratoriosCommand.CanExecute(null))
        {
            try
            {
                // Se ejecuta directo para que el framework maneje el encolado de forma óptima
                _viewModel.CargarLaboratoriosCommand.Execute(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al ejecutar CargarLaboratoriosCommand: {ex.Message}");
            }
        }

        // 3. Encendemos la escucha en tiempo real (SSE) una vez que la pantalla ya está pintada con los datos base
        try
        {
            _viewModel.IniciarEscuchaSSE();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al iniciar SSE: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // 🔥 Tu solución 'fire-and-forget': Desconexión y limpieza en un hilo secundario
        // Al no esperarlo en el hilo principal de la UI, el cambio a otra pestaña es instantáneo.
        _ = Task.Run(() =>
        {
            try
            {
                _viewModel.DetenerEscuchaSSE();
                _viewModel.LimpiarRecursos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al limpiar recursos en OnDisappearing: {ex.Message}");
            }
        });
    }
}