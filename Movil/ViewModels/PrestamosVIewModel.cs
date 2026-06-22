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
        private readonly int _userId;

        [ObservableProperty]
        private ObservableCollection<Prestamos> _prestamos = new();

        [ObservableProperty]
        private string _titulo = "Cargando mis préstamos...";

        public PrestamosViewModel()
        {
            _userId = Preferences.Default.Get("usuarioID", 0);
        }

        // --- MÉTODOS PARA REAL-TIME (SSE) ---

        public void IniciarEscuchaSSE()
        {
            if (_userId == 0)
            {
                Titulo = "No hay una sesión activa.";
                return;
            }

            Titulo = "Sincronizando préstamos en tiempo real...";
            _sseCts = new CancellationTokenSource();

            // Llamamos a un nuevo método en tu ApiService específico para préstamos
            _ = _apiService.EscucharPrestamosSSEAsync(
                _userId,
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
            // Forzamos el hilo principal para actualizar la vista sin excepciones
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Prestamos.Clear();

                if (nuevosDatos != null && nuevosDatos.Any())
                {
                    foreach (var prestamo in nuevosDatos)
                    {
                        Prestamos.Add(prestamo);
                    }
                    Titulo = "Mis Préstamos Activos";
                }
                else
                {
                    Titulo = "No tienes préstamos registrados.";
                }
            });
        }

        // Dejo tu método de carga manual por si lo necesitas para un RefreshView (Pull to refresh)
        [RelayCommand]
        private async Task CargarPrestamosAsync()
        {
            if (_userId == 0)
            {
                Titulo = "No hay una sesión activa.";
                return;
            }

            try
            {
                var listaPrestamos = await _apiService.ObtenerPrestamosPorUsuarioAsync(_userId);
                ActualizarListaUI(listaPrestamos);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
                Titulo = "Error al cargar";
            }
        }

    }
}