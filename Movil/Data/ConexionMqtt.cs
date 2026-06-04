using MQTTnet;
using MQTTnet.Client;
using Shared.Models;
using Shared.Services;

namespace Movil.Data
{
    public class ConexionMqtt
    {

        private IMqttClient mqttClient;

        // Evento para notificar a la UI cuando llega un mensaje
        public event Action<string, string> MensajeRecibido;

        public async Task ConectarAsync()
        {
            try
            {
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                // Configuración para tu Mosquitto local. 
                // Si está en otra PC o Docker, cambia "localhost" por la IP correspondiente.
                //.WithClientId("ApiCsharpCliente_" + Guid.NewGuid().ToString().Substring(0, 5)) // ID único

                var options = new MqttClientOptionsBuilder()
                    .WithClientId("ClientApp" + Guid.NewGuid().ToString().Substring(0, 5)) // ID único
                    .WithTcpServer(MqttServices.host, MqttServices.port)
                    .WithCredentials(MqttServices.Username, MqttServices.Pasword)
                    .WithCleanSession()
                    .Build();

                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    string topic = e.ApplicationMessage.Topic;
                    string payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    // Disparar el evento hacia la ventana
                    MensajeRecibido?.Invoke(topic, payload);
                    return Task.CompletedTask;
                };

                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                throw; //Colocar el error para atraparlo en la ventana
            }
        }

        public async Task SuscribirseAsync(string topico)
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                var mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => { f.WithTopic(topico); })
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
            }
        }

        public async Task PublicarMensajeAsync(string topico, string mensaje)
        {
            await ConectarAsync();

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topico)
                .WithPayload(mensaje)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        }
    }
}
