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

                StatusMensaje = "Conectando al ESP32 por MQTT...";

                await _miBroker.ConectarAsync();

                // 1. Suscripción al estatus general
                await _miBroker.SuscribirseAsync(MqttServices.statusTopic);

                // 2. ADAPTACIÓN CLAVE: Le agregamos el comodín "/#" al doorTopic
                // Esto le dice a tu app: "Escucha todo lo que empiece con UPT/LABORATORIOS/doorStatus/"
                await _miBroker.SuscribirseAsync($"{MqttServices.doorTopic}/#");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusColor = new SolidColorBrush(Colors.Green);
                    StatusMensaje = "MQTT Conectado al ESP32";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la inicialización: {ex.Message}");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMensaje = "Error de conexión MQTT";
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

            // Estatus del Gateway
            if (topic == MqttServices.statusTopic)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    bool online = payload.ToLower().Contains("online");
                    StatusColor = new SolidColorBrush(online ? Colors.Green : Colors.Red);
                    StatusMensaje = online ? "MQTT Conectado" : "MQTT Desconectado";
                });
                return;
            }

            // 3. ADAPTACIÓN DEL TOPICO DE LA PUERTA
            if (topic.StartsWith(MqttServices.doorTopic))
            {
                // Dividimos el tópico por las diagonales '/'
                // Ejemplo: "UPT/LABORATORIOS/doorStatus/LAB01" se convierte en un arreglo de 4 partes.
                string[] parts = topic.Split('/');

                // Verificamos que tenga la parte extra (el ID del laboratorio)
                if (parts.Length > 3)
                {
                    // parts[3] contendría "LAB01"
                    string idLabMqtt = parts[3];

                    // Verificamos si el mensaje indica que se abrió
                    bool abierta = payload.ToLower().Contains("abierta") ||
                                   payload.Contains("true") ||
                                   payload.Contains("1") ||
                                   payload.Contains("open");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Buscamos en nuestra lista el laboratorio que coincida con el ID que mandó el ESP32
                        // Nota: Asegúrate de que DireccionLora (o la propiedad que uses) coincida con el identificador del ESP32
                        var laboratorio = ListaLaboratorios.FirstOrDefault(l =>
                            l.DireccionLora != null &&
                            l.DireccionLora.Equals(idLabMqtt, StringComparison.OrdinalIgnoreCase));

                        if (laboratorio != null)
                        {
                            // Actualizamos la interfaz gráfica al instante
                            laboratorio.DatosPuerta.EstadoPuerta = abierta ? "Abierto" : "Cerrado";
                        }
                    });
                }
            }
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