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

namespace Escritorio.Windows
{
    //version de la vaina 1.0 y proximo actulizacion

    /// <summary>
    /// Lógica de interacción para LaboratorioWindow.xaml
    /// </summary>
    public partial class LaboratorioWindow : Window
    {
        private EscritorioMQTT miBroker;
        private DispatcherTimer watchdogTimer;

        public LaboratorioWindow()
        {
            InitializeComponent();
            miBroker = new EscritorioMQTT();
            miBroker.MensajeRecibido += MensajeRecibido;

            ConfigurarWatchdog();
            ConectarYSuscribir();
        }

        private void ConfigurarWatchdog()
        {
            watchdogTimer = new DispatcherTimer();
            watchdogTimer.Interval = TimeSpan.FromSeconds(35);
            watchdogTimer.Tick += WatchdogTimer_Tick;
        }

        private async void ConectarYSuscribir()
        {
            try
            {
                await miBroker.ConectarAsync();
                await miBroker.SuscribirseAsync(MqttServices.statusTopic);
                // Suscripción con comodín para capturar todos los laboratorios
                await miBroker.SuscribirseAsync(MqttServices.doorTopic + "/+");
            }
            catch (Exception ex)
            {
                if (MessageBox.Show("Error al conectar MQTT: " + ex.Message, "Error", MessageBoxButton.RetryCancel) == MessageBoxResult.Retry)
                    ConectarYSuscribir();
            }
        }

        private void MensajeRecibido(string topic, string payload)
        {
            if (string.IsNullOrEmpty(payload)) return;

            // Estado del Gateway
            if (topic == MqttServices.statusTopic)
            {
                Dispatcher.Invoke(() =>
                {
                    bool online = payload.ToLower().Contains("online");
                    StatusIndicator.Fill = new SolidColorBrush(online ? Colors.Green : Colors.Red);
                    lblStatus.Text = online ? "Conectado" : "Desconectado";
                });
                return;
            }

            // Procesamiento de Puertas (Carril único)
            if (topic.StartsWith(MqttServices.doorTopic))
            {
                string[] parts = topic.Split('/');
                if (parts.Length > 3)
                {
                    string idLab = parts[3];
                    bool abierta = payload.ToLower().Contains("abierta") || payload.Contains("true") || payload.Contains("1") || payload.Contains("open");

                    Dispatcher.Invoke(() =>
                    {
                        switch (idLab)
                        {
                            case "LAB:02_S1":
                                imgPuertaCerradaLab2.Visibility = abierta ? Visibility.Hidden : Visibility.Visible;
                                imgPuertaAbiertaLab2.Visibility = abierta ? Visibility.Visible : Visibility.Hidden;
                                break;
                            case "LAB:03_S1":
                                imgPuertaCerradaLab3.Visibility = abierta ? Visibility.Hidden : Visibility.Visible;
                                imgPuertaAbiertaLab3.Visibility = abierta ? Visibility.Visible : Visibility.Hidden;
                                break;
                            case "LAB:04_S1":
                                imgPuertaCerradaLab4.Visibility = abierta ? Visibility.Hidden : Visibility.Visible;
                                imgPuertaAbiertaLab4.Visibility = abierta ? Visibility.Visible : Visibility.Hidden;
                                break;
                        }

                        lblUltimoReporte.Text = $"Último reporte ({idLab}): {DateTime.Now:HH:mm:ss}";
                        lblStatusLora.Text = "Red LoRa: Operativa";
                        lblStatusLora.Foreground = new SolidColorBrush(Colors.Blue);

                        watchdogTimer.Stop();
                        watchdogTimer.Start();
                    });
                }
            }
        }

        private void WatchdogTimer_Tick(object sender, EventArgs e)
        {
            watchdogTimer.Stop();
            lblStatusLora.Text = "Nodo Inalcanzable (Pérdida de señal LoRa)";
            lblStatusLora.Foreground = new SolidColorBrush(Colors.Red);
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

        private async void btnLab2_Click(object sender, RoutedEventArgs e) => await EnviarComando("2");
        private async void btnLab3_Click(object sender, RoutedEventArgs e) => await EnviarComando("3");
        private async void btnLab4_Click(object sender, RoutedEventArgs e) => await EnviarComando("4");

        private void btnImagenregresar_Click(object sender, RoutedEventArgs e)
        {
            InicioWindow ventanaInicio = new InicioWindow();
            ventanaInicio.Show();
            this.Hide();
        }
    }
}