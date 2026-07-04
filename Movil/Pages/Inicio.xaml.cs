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

        // 1. Iniciamos la escucha SSE inmediatamente
        _viewModel.IniciarEscuchaSSE();

        // 2. Pequeña pausa para que la pestaña se acomode visualmente sin tirones
        await Task.Delay(100);

        // 3. Carga pesada en un hilo secundario
        if (_viewModel.CargarLaboratoriosCommand.CanExecute(null))
        {
            await Task.Run(() =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _viewModel.CargarLaboratoriosCommand.Execute(null);
                });
            });
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        // 🔥 LA SOLUCIÓN AL RETRASO:
        // Enviamos el cierre de conexiones y limpieza a otro hilo.
        // Al no esperarlo con 'await' aquí, la pestaña cambia EN EL ACTO.
        _ = Task.Run(() =>
        {
            try
            {
                _viewModel.DetenerEscuchaSSE();
                _viewModel.LimpiarRecursos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al limpiar: {ex.Message}");
            }
        });
    }
}