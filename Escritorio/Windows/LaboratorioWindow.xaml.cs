using Escritorio.Data;
using Shared.Models;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Threading.Tasks;

namespace Escritorio.Windows
{
    public partial class LaboratorioWindow : Window
    {
        private EscritorioMQTT miBroker;
        private readonly ApiService _apiService = new ApiService();
        private List<Laboratorios> _listaLaboratorios = new List<Laboratorios>();

        public int NombreUsuario { get; set; }

        public LaboratorioWindow()
        {
            InitializeComponent();
            miBroker = new EscritorioMQTT();
            miBroker.MensajeRecibido += MensajeRecibido;
            CargarDatosYConectar();
        }

        private async void CargarDatosYConectar()
        {
            try
            {
                lblStatus.Text = "Consultando base de datos...";

                // La API solo nos trae la lista maestra (Nombres, Direcciones Lora, etc.)
                _listaLaboratorios = await _apiService.ObtenerLaboratoriosAsync();
                icLaboratorios.ItemsSource = _listaLaboratorios;

                lblStatus.Text = "Iniciando conexión MQTT...";

                await miBroker.ConectarAsync();

                // Nos suscribimos a tus tópicos oficiales
                await miBroker.SuscribirseAsync(MqttServices.statusTopic);

                // Con el "+", le decimos que escuche cualquier laboratorio (Ej. UPT/LABORATORIOS/doorStatus/LAB:01)
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

            // 1. Estado general del Gateway
            if (topic == MqttServices.statusTopic)
            {
                Dispatcher.Invoke(() =>
                {
                    bool online = payload.ToLower().Contains("online");
                    StatusIndicator.Fill = new SolidColorBrush(online ? Colors.Green : Colors.Red);
                    lblStatus.Text = online ? "MQTT Conectado" : "MQTT Desconectado";
                });
                return;
            }

            // 2. Control de Puertas 100% por sensor (Sin tocar base de datos)
            if (topic.StartsWith(MqttServices.doorTopic))
            {
                // Cortamos el tópico: UPT / LABORATORIOS / doorStatus / ID_DEL_LAB
                string[] parts = topic.Split('/');

                // Verificamos que el arreglo tenga al menos 4 partes (índice 3)
                if (parts.Length > 3)
                {
                    string idLabMqtt = parts[3]; // Aquí se guarda el identificador, ej: "LAB:02_S1"

                    // Verificamos si el payload dice que se abrió
                    bool abierta = payload.ToLower().Contains("abierta") || payload.Contains("true") || payload.Contains("1") || payload.Contains("open");

                    Dispatcher.Invoke(() =>
                    {
                        var laboratorioModificado = _listaLaboratorios.FirstOrDefault(lab => lab.direccionLora == idLabMqtt);

                        if (laboratorioModificado != null)
                        {
                            // Actualizamos ÚNICAMENTE la memoria local para que reaccione la interfaz visual
                            laboratorioModificado.estadoPuerta = abierta ? "Abierto" : "Cerrado";
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
                // Enviamos el comando de abrir al tópico maestro ("UPT/LABORATORIOS")
                string json = JsonSerializer.Serialize(new JsonAbrir { d = idLab, c = "abrir" });
                await miBroker.PublicarMensajeAsync(MqttServices.abrir, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error enviando comando: " + ex.Message);
            }
        }

        private async void btnAbrirLab_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                var laboratorioSeleccionado = btn.DataContext as Laboratorios;
                if (laboratorioSeleccionado != null)
                {
                    await EnviarComando(laboratorioSeleccionado.ID.ToString());
                }
            }
        }

        private void btnImagenregresar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InicioWindow ventanaInicio = new InicioWindow();
                ventanaInicio.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la ventana: \n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}