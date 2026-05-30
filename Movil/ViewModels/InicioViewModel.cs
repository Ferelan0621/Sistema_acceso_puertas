// ViewModels/InicioViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Movil.ViewModels
{
    public partial class InicioViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;

        [ObservableProperty]
        private ObservableCollection<Laboratorios> laboratorios;

        public InicioViewModel()
        {
            _httpClient = new HttpClient();
            Laboratorios = new ObservableCollection<Laboratorios>();
            _ = CargarLaboratoriosAsync();
        }

        private async Task CargarLaboratoriosAsync()
        {

            try
            {
                // Aquí pones la URL de tu API
                var response = await _httpClient.GetStringAsync("https://t1tkm4dk-7153.usw3.devtunnels.ms/api/Laboratorios");
                var labs = JsonSerializer.Deserialize<List<Laboratorios>>(response);

                foreach (var lab in labs)
                {
                    Laboratorios.Add(lab);
                }
            }
            catch (Exception ex)
            {
                // Manejar error (ej. mostrar alerta)
            }
        }

        // Este comando se ejecuta al darle click a la tarjeta del laboratorio
        [RelayCommand]
        private async Task IrAPrestamoAsync(Laboratorios labSeleccionado)
        {
            if (labSeleccionado == null) return;

            // Pasamos el laboratorio completo a la vista de Préstamos
            var navigationParameter = new Dictionary<string, object>
        {
            { "LabSeleccionado", labSeleccionado }
        };

            // Asegúrate de tener la ruta registrada en tu AppShell
            await Shell.Current.GoToAsync(nameof(Prestamos), navigationParameter);
        }
    }
}