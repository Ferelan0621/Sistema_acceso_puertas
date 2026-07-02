using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escritorio.Data;
using Shared.Models;
using Shared.Services;
using Escritorio.Windows;
using System.Linq;
using System.Threading.Tasks;

namespace Escritorio.ViewModel
{
    public partial class PeticionesViewModel : ObservableObject
    {
        private readonly EscritorioMQTT _miBroker = SharedData.Instance.Broker;
        private readonly ApiService _apiService = new ApiService();

        public ObservableCollection<Laboratorios> ListaLaboratorios => SharedData.Instance.ListaLaboratorios;
        public ObservableCollection<PeticionMovil> ListaPeticiones { get; set; }

        public PeticionesViewModel()
        {
            ListaPeticiones = new ObservableCollection<PeticionMovil>();
            InicializarMQTT();
        }

        private async void InicializarMQTT()
        {
            _miBroker.MensajeRecibido += MqttClient_MensajeRecibido;
            System.Diagnostics.Debug.WriteLine("[PETICIONES] Evento MensajeRecibido enganchado.");

            try
            {
                await _miBroker.ConectarAsync();
                System.Diagnostics.Debug.WriteLine($"[PETICIONES] Suscribiéndose a tópico: '{MqttServices.conexion}'");
                await _miBroker.SuscribirseAsync(MqttServices.conexion);
                System.Diagnostics.Debug.WriteLine("[PETICIONES] Suscripción exitosa.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PETICIONES] Error MQTT: {ex.Message}");
                MessageBox.Show($"Error al conectar al broker MQTT: {ex.Message}");
            }
        }

        private void MqttClient_MensajeRecibido(string topic, string payload)
        {
            System.Diagnostics.Debug.WriteLine($"[PETICIONES] Mensaje recibido - Topic: '{topic}' | Payload: '{payload}'");

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (topic == MqttServices.conexion)
                    {
                        var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var nuevaPeticion = JsonSerializer.Deserialize<PeticionMovil>(payload, opcionesJson);

                        if (nuevaPeticion != null)
                        {
                            // 🔑 Petición de CIERRE
                            if (!string.IsNullOrEmpty(nuevaPeticion.Estatus) && nuevaPeticion.Estatus.ToLower() == "cierre")
                            {
                                _ = ResponderCierreAsync(nuevaPeticion);
                            }
                            // 🔑 Petición de ACCESO normal
                            else
                            {
                                ListaPeticiones.Add(nuevaPeticion);

                                // 🚀 Llamamos al método que busca los datos y lanza la ventana
                                _ = MostrarVentanaPeticionAsync(nuevaPeticion);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PETICIONES] ❌ Error: {ex.Message}");
                }
            });
        }

        // 🚀 NUEVO MÉTODO: Arma la info y lanza el Dialog
        private async Task MostrarVentanaPeticionAsync(PeticionMovil nuevaPeticion)
        {
            try
            {
                // Traemos la info pesada de forma asíncrona
                var usuario = await _apiService.ObtenerUsuarioPorIdAsync(nuevaPeticion.UsuarioID);
                var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == nuevaPeticion.LaboratorioID);

                string nombreUser = usuario?.Nombre ?? $"Usuario ID: {nuevaPeticion.UsuarioID}";
                string rolUser = usuario != null ? usuario.Rol.ToString() : "Sin Asignar";
                string nombreLab = lab?.NombreLaboratorio ?? $"Laboratorio {nuevaPeticion.LaboratorioID}";

                // Mostramos la tarjeta del lab en "Pendiente" mientras decides
                if (lab != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lab.DatosPuerta.UsuarioNombre = nombreUser;
                        lab.DatosPuerta.Cargo = "Esperando aprobación...";
                        lab.DatosPuerta.HoraInicio = nuevaPeticion.FechaPrestamo;
                        lab.OnPropertyChanged(nameof(lab.DatosPuerta));
                    });
                }

                // Disparamos la ventana flotante en el hilo visual
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var ventana = new PeticionDialogoWindow(nuevaPeticion, this, nombreUser, rolUser, nombreLab);
                    ventana.ShowDialog(); // ShowDialog() hace que parpadee si tocas fuera, enfocando tu atención
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VENTANA] ❌ Error al procesar datos: {ex.Message}");
            }
        }

        private async Task ResponderCierreAsync(PeticionMovil peticion)
        {
            try
            {
                var cierrePayload = new { estatus = "cierre", laboratorioID = peticion.LaboratorioID, mensaje = "Laboratorio cerrado correctamente" };
                string jsonCierre = JsonSerializer.Serialize(cierrePayload);
                string topicoDestino = $"{MqttServices.respuesta}/{peticion.UsuarioID}";

                await _miBroker.PublicarMensajeAsync(topicoDestino, jsonCierre);

                var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);
                if (lab != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lab.DatosPuerta.UsuarioNombre = string.Empty;
                        lab.DatosPuerta.Cargo = string.Empty;
                        lab.DatosPuerta.HoraInicio = string.Empty;
                        lab.DatosPuerta.EstadoPuerta = "Cerrado";
                        lab.OnPropertyChanged(nameof(lab.DatosPuerta));
                    });
                }
            }
            catch (Exception ex) { /* Manejo de error */ }
        }

        [RelayCommand]
        private async void AceptarPeticion(PeticionMovil peticion)
        {
            if (peticion == null) return;
            try
            {
                var usuario = await _apiService.ObtenerUsuarioPorIdAsync(peticion.UsuarioID);
                var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);

                var respuestaPayload = new
                {
                    estatus = "aceptado",
                    usuarioID = peticion.UsuarioID,
                    laboratorioID = peticion.LaboratorioID,
                    nombreLaboratorio = lab?.NombreLaboratorio ?? $"Laboratorio {peticion.LaboratorioID}",
                    direccionLora = lab?.DireccionLora ?? string.Empty,
                    mensaje = "Acceso concedido"
                };

                string jsonRespuesta = JsonSerializer.Serialize(respuestaPayload);
                string topicoDestino = $"{MqttServices.respuesta}/{peticion.UsuarioID}";
                await _miBroker.PublicarMensajeAsync(topicoDestino, jsonRespuesta);

                string jsonAbrir = JsonSerializer.Serialize(new { d = peticion.LaboratorioID.ToString(), c = "abrir" });
                await _miBroker.PublicarMensajeAsync(MqttServices.abrir, jsonAbrir);

                if (lab != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lab.DatosPuerta.UsuarioNombre = usuario?.Nombre ?? $"Usuario #{peticion.UsuarioID}";
                        lab.DatosPuerta.Cargo = usuario != null ? usuario.Rol.ToString() : "Sin asignar";
                        lab.DatosPuerta.EstadoPuerta = "Abierto";
                        lab.OnPropertyChanged(nameof(lab.DatosPuerta));
                    });
                }

                MessageBox.Show("Acceso Aprobado con éxito.", "Listo");
                ListaPeticiones.Remove(peticion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar respuesta al móvil: {ex.Message}");
            }
        }

        [RelayCommand]
        private async void DenegarPeticion(PeticionMovil peticion)
        {
            if (peticion == null) return;
            try
            {
                var respuestaPayload = new
                {
                    estatus = "denegado",
                    usuarioID = peticion.UsuarioID,
                    laboratorioID = peticion.LaboratorioID,
                    mensaje = $"No se autorizó tu acceso al Laboratorio {peticion.LaboratorioID}."
                };

                string jsonRespuesta = JsonSerializer.Serialize(respuestaPayload);
                string topicoDestino = $"{MqttServices.respuesta}/{peticion.UsuarioID}";
                await _miBroker.PublicarMensajeAsync(topicoDestino, jsonRespuesta);

                var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);
                if (lab != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lab.DatosPuerta.UsuarioNombre = string.Empty;
                        lab.DatosPuerta.Cargo = string.Empty;
                        lab.DatosPuerta.HoraInicio = string.Empty;
                        lab.DatosPuerta.EstadoPuerta = "Cerrado";
                        lab.OnPropertyChanged(nameof(lab.DatosPuerta));
                    });
                }

                MessageBox.Show("Acceso Rechazado correctamente.", "Listo");
                ListaPeticiones.Remove(peticion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar respuesta al móvil: {ex.Message}");
            }
        }
    }
}