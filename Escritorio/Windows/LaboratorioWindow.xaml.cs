using Escritorio.Data;
using Shared.Models;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace Escritorio.Windows
{
    public partial class LaboratorioWindow : Window
    {
        private EscritorioMQTT miBroker;
        private readonly ApiService _apiService = new ApiService();

        // Esta es la lista maestra que controlará lo que se ve en pantalla
        private List<Laboratorios> _listaLaboratorios = new List<Laboratorios>();

        public int NombreUsuario { get; set; }

        public LaboratorioWindow()
        {
            InitializeComponent();
            miBroker = new EscritorioMQTT();
            miBroker.MensajeRecibido += MensajeRecibido;

            // Primero cargamos la UI con la base de datos, luego conectamos MQTT
            CargarDatosYConectar();
        }

        private async void CargarDatosYConectar()
        {
            try
            {
                lblStatus.Text = "Consultando base de datos...";

                // 1. Traemos los laboratorios de la base de datos (API)
                _listaLaboratorios = await _apiService.ObtenerLaboratoriosAsync();

                // 2. Pintamos la interfaz asignando la lista al ItemsControl
                icLaboratorios.ItemsSource = _listaLaboratorios;

                lblStatus.Text = "Iniciando conexión MQTT...";

                // 3. Ya con los datos listos, conectamos el broker
                await miBroker.ConectarAsync();
                await miBroker.SuscribirseAsync(MqttServices.statusTopic);
                await miBroker.SuscribirseAsync(MqttServices.doorTopic + "/+");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la inicialización: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (MessageBox.Show("¿Reintentar conexión MQTT?", "Aviso", MessageBoxButton.RetryCancel) == MessageBoxResult.Retry)
                {
                    CargarDatosYConectar();
                }
            }
        }

        private void MensajeRecibido(string topic, string payload)
        {
            if (string.IsNullOrEmpty(payload)) return;

            // Estado general del Gateway
            if (topic == MqttServices.statusTopic)
            {
                Dispatcher.Invoke(() =>
                {
                    bool online = payload.ToLower().Contains("online");
                    // Asumiendo que StatusIndicator sigue en el XAML principal (fuera del ItemsControl)
                    StatusIndicator.Fill = new SolidColorBrush(online ? Colors.Green : Colors.Red);
                    lblStatus.Text = online ? "MQTT Conectado" : "MQTT Desconectado";
                });
                return;
            }

            // Procesamiento de Puertas Dinámico
            if (topic.StartsWith(MqttServices.doorTopic))
            {
                string[] parts = topic.Split('/');
                if (parts.Length > 3)
                {
                    string idLabMqtt = parts[3]; // Ej. "LAB:02_S1"
                    bool abierta = payload.ToLower().Contains("abierta") || payload.Contains("true") || payload.Contains("1") || payload.Contains("open");

                    Dispatcher.Invoke(() =>
                    {
                        // Buscamos cuál laboratorio de nuestra lista coincide con el ID que mandó MQTT.
                        // OJO: Aquí estoy asumiendo que guardaste "LAB:02_S1" en la propiedad 'direccionLora'.
                        var laboratorioModificado = _listaLaboratorios.FirstOrDefault(lab => lab.direccionLora == idLabMqtt);

                        if (laboratorioModificado != null)
                        {
                            // Actualizamos el estatus en el modelo
                            // Asegúrate de que el Enum corresponda. (Ej. 1 para Abierto, 0 para Cerrado, o como lo tengas)
                            // Si 'estatus' es un Enum de tu modelo, ajústalo según corresponda. Si es un Enum 'EstadoLaboratorio', podría ser algo así:
                            // laboratorioModificado.estatus = abierta ? EstadoLaboratorio.Abierto : EstadoLaboratorio.Cerrado;

                            // TRUCO: Como la lista no avisa automáticamente a la interfaz que un valor interno cambió, forzamos la actualización visual:
                            icLaboratorios.Items.Refresh();
                        }
                    });
                }
            }
        }

        private async Task EnviarComando(string idLab)
        {
            try
            {
                string json = JsonSerializer.Serialize(new JsonAbrir { d = idLab, c = "abrir" });
                await miBroker.PublicarMensajeAsync(MqttServices.abrir, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error enviando comando: " + ex.Message);
            }
        }

        // Este evento reemplaza a btnLab2_Click, btnLab3_Click, etc.
        private async void btnAbrirLab_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                // Extraemos todo el objeto "Laboratorios" directamente del botón que se presionó
                var laboratorioSeleccionado = btn.DataContext as Laboratorios;

                if (laboratorioSeleccionado != null)
                {
                    // Mandamos el comando usando el ID del modelo. Si tu ESP32 usa la dirección Lora, cámbialo a .direccionLora
                    await EnviarComando(laboratorioSeleccionado.ID.ToString());
                }
            }
        }

        private void btnImagenregresar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Este bloque try-catch te ayudará a ver por qué te daba error al regresar de ventana.
                InicioWindow ventanaInicio = new InicioWindow();
                ventanaInicio.Show();
                this.Close(); // Es mejor Close() que Hide() para liberar memoria si no vas a reutilizar la misma instancia
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la ventana de inicio: \n{ex.Message}\n\nDetalles técnicos:\n{ex.StackTrace}",
                                "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}