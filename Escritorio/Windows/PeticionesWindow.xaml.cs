using Escritorio.Data;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Escritorio.Windows
{
    /// <summary>
    /// Lógica de interacción para PeticionesWindow.xaml
    /// </summary>
    public partial class PeticionesWindow : Window
    {
        private EscritorioMQTT _clienteMqtt;
        public string nombreEncargado;
        public PeticionesWindow()
        {
            InitializeComponent();

            _clienteMqtt = new EscritorioMQTT();
            _ = IniciarComunicacionMqtt();
        }
        private async Task IniciarComunicacionMqtt()
        {
            try
            {
                // 1. Nos suscribimos al evento que creaste en EscritorioMQTT.cs
                _clienteMqtt.MensajeRecibido += AlRecibirMensajeMqtt;

                // 2. Nos conectamos al broker
                await _clienteMqtt.ConectarAsync();

                // 3. Nos suscribimos al tópico específico del móvil (el que estaba en azul)
                await _clienteMqtt.SuscribirseAsync(MqttServices.conexion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar MQTT: {ex.Message}");
            }
        }

        // Este método se dispara gracias al "MensajeRecibido?.Invoke(topic, payload);" de tu clase
        private void AlRecibirMensajeMqtt(string topic, string payload)
        {
            // Verificamos que el mensaje venga del tópico del móvil
            if (topic == MqttServices.conexion)
            {
                // RECUERDA: Pasamos al hilo principal para poder modificar la interfaz (ej. tus PNGs en el Canvas)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Suponiendo que el móvil manda la palabra "abrir"
                    if (payload == "abrir")
                    {
                        MessageBox.Show("Petición del móvil: ¡Abrir puerta!");

                        // Aquí puedes actualizar el Canvas
                        // MiImagenPuerta.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Puerta_abierta.png"));
                    }
                });
            }
        }

        private void btnHistorialpeticiones_Click(object sender, RoutedEventArgs e)
        {
            HistorialpeticionesWindow ventanaHistorial = new HistorialpeticionesWindow();
            ventanaHistorial.Show();
            this.Hide();
        }
        private void btnImagenregresar_Click(object sender, RoutedEventArgs e)
        {
            InicioWindow ventanaInicio = new InicioWindow();
            ventanaInicio.Show();
            this.Hide();
        }
    }
}
