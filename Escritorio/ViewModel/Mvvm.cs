using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escritorio.Data;
using Shared.Models;
using Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Threading;

namespace Escritorio.Mvvm
{
    public partial class LaboratorioViewModel : ObservableObject
    {
        private readonly ApiService _apiService = new ApiService();
        private readonly EscritorioMQTT _miBroker;
        private CancellationTokenSource _sseCancellationTokenSource;

        // Esta lista avisa a la UI cuando se agregan o quitan elementos
        [ObservableProperty]
        private ObservableCollection<Laboratorios> _listaLaboratorios = new();

        // Controlamos el texto y el color del foquito desde aquí
        [ObservableProperty]
        private string _statusMensaje = "Esperando conexión...";

        [ObservableProperty]
        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Gray);

        public LaboratorioViewModel()
        {
            _miBroker = new EscritorioMQTT();
            _miBroker.MensajeRecibido += MensajeRecibido;

            // Disparamos la carga asíncrona sin bloquear el constructor
            _ = CargarDatosYConectarAsync();
        }

        private async Task CargarDatosYConectarAsync()
        {
            try
            {
                StatusMensaje = "Consultando BD...";
                var labs = await _apiService.ObtenerLaboratoriosAsync();

                if (labs != null)
                {
                    ListaLaboratorios = new ObservableCollection<Laboratorios>(labs);
                }

                StatusMensaje = "Conectando al stream SSE...";

                // Inicializamos el token para poder apagar la conexión cuando se cierre la ventana
                _sseCancellationTokenSource = new CancellationTokenSource();

                // Lanzamos la escucha de SSE en segundo plano
                _ = _apiService.EscucharPuertasSSEAsync(ProcesarEventoPuerta, _sseCancellationTokenSource.Token);

                // Indicador visual de que estamos conectados (puedes ajustarlo según tu lógica)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusColor = new SolidColorBrush(Colors.Green);
                    StatusMensaje = "SSE Conectado y escuchando en vivo";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la inicialización: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMensaje = "Error de conexión";
            }
        }

        private void ProcesarEventoPuerta(SensorPuerta evento)
        {
            if (evento == null || string.IsNullOrEmpty(evento.SensorMqtt)) return;

            bool abierta = evento.Estado.ToLower().Contains("abierta") ||
                           evento.Estado.Contains("true") ||
                           evento.Estado.Contains("1") ||
                           evento.Estado.Contains("open");

            Application.Current.Dispatcher.Invoke(() =>
            {
                var laboratorio = ListaLaboratorios.FirstOrDefault(l =>
                    l.DireccionLora != null &&
                    l.DireccionLora.Equals(evento.SensorMqtt, StringComparison.OrdinalIgnoreCase));

                if (laboratorio != null)
                {
                    // OJO AQUÍ: Ahora actualizamos la propiedad interna "DatosPuerta"
                    laboratorio.DatosPuerta.EstadoPuerta = abierta ? "Abierto" : "Cerrado";
                }
            });
        }

        private void MensajeRecibido(string topic, string payload)
        {
            if (string.IsNullOrEmpty(payload)) return;

            // 1. Estatus del Gateway
            if (topic == MqttServices.statusTopic)
            {
                // Dispatcher para que no crashee por actualizar la UI desde otro hilo
                Application.Current.Dispatcher.Invoke(() =>
                {
                    bool online = payload.ToLower().Contains("online");
                    StatusColor = new SolidColorBrush(online ? Colors.Green : Colors.Red);
                    StatusMensaje = online ? "MQTT Conectado" : "MQTT Desconectado";
                });
                return;
            }

            // 2. Sensores de las puertas
            /*if (topic.StartsWith(MqttServices.doorTopic))
            {
                string[] parts = topic.Split('/');
                if (parts.Length > 3)
                {
                    string idLabMqtt = parts[3];
                    bool abierta = payload.ToLower().Contains("abierta") || payload.Contains("true") || payload.Contains("1") || payload.Contains("open");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var laboratorio = ListaLaboratorios.FirstOrDefault(l =>
                            l.DireccionLora != null &&
                            l.DireccionLora.Equals(idLabMqtt, StringComparison.OrdinalIgnoreCase));

                        if (laboratorio != null)
                        {
                            // MAGIA MVVM: Solo cambias esto y la tarjeta en XAML se actualiza sola
                            laboratorio.EstadoPuerta = abierta ? "Abierto" : "Cerrado";
                        }
                    });
                }
            }*/
        }

        // Este Comando reemplaza al viejo evento btnAbrirLab_Click
        [RelayCommand]
        private async Task AbrirLabAsync(Laboratorios labSeleccionado)
        {
            if (labSeleccionado == null) return;

            try
            {
                string json = JsonSerializer.Serialize(new { d = labSeleccionado.ID.ToString(), c = "abrir" });
                await _miBroker.PublicarMensajeAsync(MqttServices.abrir, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error enviando comando: " + ex.Message);
            }
        }
        // Llama a este método desde el evento "Closed" de tu ventana o cuando el usuario regrese al inicio
        public void DesconectarSSE()
        {
            _sseCancellationTokenSource?.Cancel();
        }
    }
}