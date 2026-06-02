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

namespace Escritorio.Windows
{
    //version de la vaina 1.0 y proximo actulizacion

    /// <summary>
    /// Lógica de interacción para LaboratorioWindow.xaml
    /// </summary>
    public partial class LaboratorioWindow : Window
    {
        private EscritorioMQTT miBroker;
        public LaboratorioWindow()
        {
            InitializeComponent();
            // Inicializar el broker y comenzar la conexión
            miBroker = new EscritorioMQTT();

            //Asignar el manejador de mensajes antes de conectar
            miBroker.MensajeRecibido += MensajeRecibido;
            ConectarYSuscribir();
        }

        private async void ConectarYSuscribir()
        {
            try
            {
                await miBroker.ConectarAsync();
                await miBroker.SuscribirseAsync(MqttServices.statusTopic);
                await miBroker.SuscribirseAsync(MqttServices.doorTopic);
            }
            catch (Exception ex)
            {
                // Mostramos el mensaje y capturamos el resultado del botón presionado
                MessageBoxResult resultado = MessageBox.Show(
                    "Error al inicializar la conexión MQTT: " + ex.Message,
                    "Error de Conexión",
                    MessageBoxButton.RetryCancel,
                    MessageBoxImage.Error);

                // Si el usuario presionó "Reintentar", llamamos a este mismo método de nuevo
                if (resultado == MessageBoxResult.Retry)
                {
                    ConectarYSuscribir();
                }
            }
        }

        private void MensajeRecibido(string topic, string payload)
        {
            // Protección por si el ESP32 manda algo vacío
            if (string.IsNullOrEmpty(payload)) return;

            // 1. Validamos el tópico de estado de conexión de la app
            if (topic == MqttServices.statusTopic)
            {
                Dispatcher.Invoke(() =>
                {
                    if (payload.ToLower().Contains("online"))
                    {
                        StatusIndicator.Fill = new SolidColorBrush(Colors.Green);
                        lblStatus.Text = "Conectado";
                    }
                    else
                    {
                        StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
                        lblStatus.Text = "Desconectado";
                    }
                });
            }
            // Validamos el tópico de la puerta
            /*if (topic == MqttServices.doorTopic)
            {
                // Convertimos todo el mensaje a minúsculas para evitar errores de formato
                string mensajeLimpio = payload.ToLower();

                Dispatcher.Invoke(() =>
                {
                    // Evaluamos todas las posibles formas en que tu ESP32 pueda responder
                    if (mensajeLimpio.Contains("abierta") || mensajeLimpio.Contains("true") || mensajeLimpio.Contains("1") || mensajeLimpio.Contains("open"))
                    {
                        imgPuertaCerradaLab2.Visibility = Visibility.Hidden;
                        imgPuertaAbiertaLab2.Visibility = Visibility.Visible;
                    }
                    // Si el sensor detecta que se cerró
                    else if (mensajeLimpio.Contains("cerrada") || mensajeLimpio.Contains("false") || mensajeLimpio.Contains("0") || mensajeLimpio.Contains("close"))
                    {
                        imgPuertaAbiertaLab2.Visibility = Visibility.Hidden;
                        imgPuertaCerradaLab2.Visibility = Visibility.Visible;
                    }
                });
            }*/
        }

        private void btnNuevolaboratorio_Click(object sender, RoutedEventArgs e)
        {
            NuevolaboratorioWindow ventaNuevoLab = new NuevolaboratorioWindow();
            ventaNuevoLab.Show();
            this.Hide();
        }

        private async void btnLab2_Click(object sender, RoutedEventArgs e)
        {
            JsonAbrir jsonlab2 = new JsonAbrir
            { d = "2", c = "abrir" };

            string Jsonlab2 = JsonSerializer.Serialize(jsonlab2);

            // Validamos que el elemento principal (el broker) no falte antes de intentar enviar
            if (miBroker == null)
            {
                MessageBox.Show("Falta inicializar el cliente MQTT. No se puede enviar la petición.", "Elemento Faltante", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Publicamos el mensaje "2" en el tópico definido en MqttServices.abrir ("UPT/LABORATORIOS")
                await miBroker.PublicarMensajeAsync(MqttServices.abrir, Jsonlab2);

                // Confirmación visual opcional
                MessageBox.Show("La petición de apertura se envió correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error en caso de que falte conexión de red o falle el envío
                MessageBox.Show("Falta conexión o hubo un error al enviar el mensaje: " + ex.Message, "Error de Envío", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnLab3_Click(object sender, RoutedEventArgs e)
        {
            JsonAbrir jsonlab3 = new JsonAbrir
            { d = "3", c = "abrir" };

            string Jsonlab3 = JsonSerializer.Serialize(jsonlab3);

            // Validamos que el elemento principal (el broker) no falte antes de intentar enviar
            if (miBroker == null)
            {
                MessageBox.Show("Falta inicializar el cliente MQTT. No se puede enviar la petición.", "Elemento Faltante", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Publicamos el mensaje "3" en el tópico definido en MqttServices.abrir ("UPT/LABORATORIOS")
                await miBroker.PublicarMensajeAsync(MqttServices.abrir, Jsonlab3);

                // Confirmación visual opcional
                MessageBox.Show("La petición de apertura se envió correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error en caso de que falte conexión de red o falle el envío
                MessageBox.Show("Falta conexión o hubo un error al enviar el mensaje: " + ex.Message, "Error de Envío", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnLab4_Click(object sender, RoutedEventArgs e)
        {
            JsonAbrir jsonlab4 = new JsonAbrir
            { d = "4", c = "abrir" };

            string Jsonlab4 = JsonSerializer.Serialize(jsonlab4);

            // Validamos que el elemento principal (el broker) no falte antes de intentar enviar
            if (miBroker == null)
            {
                MessageBox.Show("Falta inicializar el cliente MQTT. No se puede enviar la petición.", "Elemento Faltante", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Publicamos el mensaje "4" en el tópico definido en MqttServices.abrir ("UPT/LABORATORIOS")
                await miBroker.PublicarMensajeAsync(MqttServices.abrir, Jsonlab4);

                // Confirmación visual opcional
                MessageBox.Show("La petición de apertura se envió correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error en caso de que falte conexión de red o falle el envío
                MessageBox.Show("Falta conexión o hubo un error al enviar el mensaje: " + ex.Message, "Error de Envío", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
