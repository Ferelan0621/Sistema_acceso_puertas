using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Escritorio.Data;
using Shared.Models;
using Shared.Services;

namespace Escritorio.ViewModel
{
    // Hacemos la clase public y partial, y heredamos de ObservableObject
    public partial class HistorialPeticionesViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly EscritorioMQTT _mqttClient;

        // La colección que se enlazará a la vista
        [ObservableProperty]
        private ObservableCollection<Prestamos> _listaHistorial = new();

        public HistorialPeticionesViewModel()
        {
            _apiService = new ApiService();
            _mqttClient = new EscritorioMQTT();

            // Iniciamos la carga de la BD y la escucha MQTT
            _ = InicializarHistorialAsync();
        }

        private async Task InicializarHistorialAsync()
        {
            // 1. Cargar el historial guardado desde tu Base de Datos usando tu API
            await CargarDesdeBDAsync();

            // 2. Conectar MQTT para el "Tiempo Real"
            _mqttClient.MensajeRecibido += MqttClient_MensajeRecibido;
            try
            {
                await _mqttClient.ConectarAsync();
                // Nos suscribimos al tópico donde publicas si fue aceptado o denegado
                await _mqttClient.SuscribirseAsync(MqttServices.respuesta);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MQTT en historial: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task CargarDesdeBDAsync()
        {
            try
            {
                // NOTA: Aquí asumo que crearás un método en tu ApiService para traer el historial
                 var datos = await _apiService.ObtenerHistorialPrestamosAsync();
                 ListaHistorial = new ObservableCollection<Prestamos>(datos);

                // Mientras tanto, para que no marque error:
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}");
            }
        }

        private void MqttClient_MensajeRecibido(string topic, string payload)
        {
            // Si llega un mensaje de respuesta (Aceptado/Denegado) desde la otra ventana
            if (topic == MqttServices.respuesta)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Creamos un registro temporal en la UI para el efecto "Tiempo Real"
                    // (La persistencia real se hace en la otra ventana al llamar a la API)
                    var nuevoRegistro = new Prestamos
                    {
                        FechaSolicitud = DateTime.Now,
                        // Aquí tendrías que parsear el payload para sacar el ID del usuario y lab real
                        // Por ahora lo ponemos visualmente para que veas que funciona:
                    };

                    // Lo insertamos al inicio de la lista para que sea el primero en verse
                    ListaHistorial.Insert(0, nuevoRegistro);
                });
            }
        }
    }
}