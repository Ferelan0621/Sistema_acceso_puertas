using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Escritorio.Data;
using Shared.Models;
using Shared.Services;     // ¡CRÍTICO! Agregado para acceder a MqttServices

namespace Escritorio.ViewModel
{
    public class PeticionesViewModel
    {
        private EscritorioMQTT _mqttClient;

        // Propiedad que el XAML va a leer en tiempo real
        public ObservableCollection<PeticionMovil> ListaPeticiones { get; set; }

        // Declaración de Comandos para los botones Aceptar y Denegar
        public ICommand AceptarCommand { get; }
        public ICommand DenegarCommand { get; }

        public PeticionesViewModel()
        {
            ListaPeticiones = new ObservableCollection<PeticionMovil>();

            // Vinculamos los comandos
            AceptarCommand = new RelayCommand<PeticionMovil>(AceptarPeticion);
            DenegarCommand = new RelayCommand<PeticionMovil>(DenegarPeticion);

            InicializarMQTT();
        }

        private async void InicializarMQTT()
        {
            _mqttClient = new EscritorioMQTT();
            _mqttClient.MensajeRecibido += MqttClient_MensajeRecibido;

            try
            {
                await _mqttClient.ConectarAsync();

                // Ahora el escritorio escucha exactamente donde el móvil publica
                string topicoSuscripcion = MqttServices.conexion;
                await _mqttClient.SuscribirseAsync(topicoSuscripcion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar al broker MQTT: {ex.Message}");
            }
        }

        private void MqttClient_MensajeRecibido(string topic, string payload)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var nuevaPeticion = JsonSerializer.Deserialize<PeticionMovil>(payload, opcionesJson);

                    if (nuevaPeticion != null)
                    {
                        ListaPeticiones.Add(nuevaPeticion);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al procesar JSON: {ex.Message}");
                }
            });
        }

        // Se cambia a async void para poder usar el await al publicar
        private async void AceptarPeticion(PeticionMovil peticion)
        {
            if (peticion == null) return;

            try
            {
                // 1. Avisamos al móvil que fue aceptado publicando en su tópico de escucha
                string mensajeRespuesta = $"Aceptado: Acceso concedido al Laboratorio {peticion.LaboratorioID}.";
                await _mqttClient.PublicarMensajeAsync(MqttServices.respuesta, mensajeRespuesta);

                // 2. Lógica local en el escritorio (puedes meter aquí tu ApiService)
                MessageBox.Show($"Has ACEPTADO el acceso al Lab {peticion.LaboratorioID} para el usuario {peticion.UsuarioID}", "Aprobado");

                // 3. Quitamos de la lista
                ListaPeticiones.Remove(peticion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar respuesta al móvil: {ex.Message}");
            }
        }

        // Se cambia a async void para poder usar el await al publicar
        private async void DenegarPeticion(PeticionMovil peticion)
        {
            if (peticion == null) return;

            try
            {
                // 1. Avisamos al móvil que fue rechazado
                string mensajeRespuesta = $"Denegado: No se autorizó el acceso al Laboratorio {peticion.LaboratorioID}.";
                await _mqttClient.PublicarMensajeAsync(MqttServices.respuesta, mensajeRespuesta);

                // 2. Lógica local
                MessageBox.Show($"Has DENEGADO el acceso al Lab {peticion.LaboratorioID}.", "Rechazado");

                // 3. Quitamos de la lista
                ListaPeticiones.Remove(peticion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar respuesta al móvil: {ex.Message}");
            }
        }
    }

    // --- CLASE AUXILIAR PARA MVVM (Sin cambios, se queda igual) ---
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
        public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}