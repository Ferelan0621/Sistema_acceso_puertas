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
            ConectarBrokerAsincrono();
        }

        private async void ConectarBrokerAsincrono()
        {
            try
            {
                await miBroker.ConectarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al inicializar la conexión MQTT: " + ex.Message, "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                // Publicamos el mensaje "2" en el tópico definido en MqttServices.abrir ("UPT/LABORATORIOS")
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
                // Publicamos el mensaje "2" en el tópico definido en MqttServices.abrir ("UPT/LABORATORIOS")
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
