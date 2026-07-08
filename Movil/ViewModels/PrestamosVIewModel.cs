using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Movil.Data;
using Movil.Services;
using Shared.Models;
using Shared.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Movil.ViewModels
{
    public partial class PrestamosViewModel : ObservableObject
    {

        private CancellationTokenSource _sseCts;

        private readonly ApiService _apiService = new ApiService();

        // Propiedades observables automáticas gracias al Toolkit
        [ObservableProperty]
        private ObservableCollection<Prestamos> _prestamos = new();

        [ObservableProperty]
        private string _titulo = "Cargando...";

        [ObservableProperty]
        private string _resultadoJson;

        [ObservableProperty]
        private Laboratorios _laboratorioActual;

        [ObservableProperty]
        private Laboratorios _laboratorioSeleccionado;

        public PrestamosViewModel()
        {

        }
        private int _userId => Preferences.Default.Get("usuarioID", 0);


        public void IniciarEscuchaSSE()
        {
            if (_userId == 0) return;

            _sseCts = new CancellationTokenSource();

            // Le pasamos el _userId al servicio para que el backend sepa a quién filtrar
            _ = _apiService.EscucharPrestamosSSEAsync(_userId,
                datosNuevos => ActualizarListaUI(datosNuevos),
                _sseCts.Token
            );
        }

        public void DetenerEscuchaSSE()
        {
            _sseCts?.Cancel();
            _sseCts?.Dispose();
        }

        private void ActualizarListaUI(List<Prestamos> nuevosDatos)
        {
            // ¡VITAL! Todo lo que actualice la UI desde un evento SSE debe ir al MainThread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Prestamos.Clear();

                if (nuevosDatos != null && nuevosDatos.Any())
                {
                    foreach (var pres in nuevosDatos)
                    {
                        Prestamos.Add(pres);
                    }
                    Titulo = "Historial de Prestamos";
                }
                else
                {
                    Titulo = "No hay préstamos activos";
                }
            });

        }


        // Agrega este método a tu ViewModel
        public async Task InicializarPantallaAsync()
        {
            // 1. Traemos los datos que ya existen usando tu método actual
            await CargarPrestamosAsync();

            // 2. Nos conectamos al broker/API para escuchar los futuros cambios
            IniciarEscuchaSSE();
        }

        [RelayCommand]
        public async Task CargarPrestamosAsync()
        {
            if (_userId == 0) return;

            try
            {
                var listaPrest = await _apiService.ObtenerPrestamosPorUsuariosAsync(_userId);
                ActualizarListaUI(listaPrest ?? new List<Prestamos>());
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
                Titulo = "Error de conexión";
            }
        }
    }
}


    


        
      