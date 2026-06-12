using System;
using System.Collections.Generic;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using Shared.Services;

namespace Escritorio.Data
{
    public class EscritorioMQTT
    {
        private IMqttClient? mqttClient;

        // Evento para notificar a la UI cuando llega un mensaje
        public event Action<string, string> MensajeRecibido;

        public async Task ConectarAsync()
        {
            try
            {
                // Evita intentar conectarse si ya estamos conectados
                if (mqttClient != null && mqttClient.IsConnected)
                    return;

                // Solo inicializamos el cliente si no existe
                if (mqttClient == null)
                {
                    var factory = new MqttFactory();
                    mqttClient = factory.CreateMqttClient();

                    // Suscribimos el evento SOLO UNA VEZ al crear el cliente
                    mqttClient.ApplicationMessageReceivedAsync += e =>
                    {
                        string topic = e.ApplicationMessage.Topic;
                        string payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                        // Disparar el evento hacia la ventana
                        MensajeRecibido?.Invoke(topic, payload);
                        return Task.CompletedTask;
                    };
                }

                var options = new MqttClientOptionsBuilder()
                    .WithClientId("ClientApp" + Guid.NewGuid().ToString().Substring(0, 5)) // ID único
                    .WithTcpServer(MqttServices.host, MqttServices.port)
                    .WithCredentials(MqttServices.Username, MqttServices.Pasword)
                    .WithCleanSession()
                    .Build();

                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                // Dejamos que el error suba a la ventana para que el try-catch de ahí lo atrape
                throw;
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
            // Validamos: si no existe el cliente o se desconectó, entonces sí nos conectamos
            if (mqttClient == null || !mqttClient.IsConnected)
            {
                await ConectarAsync();
            }

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topico)
                .WithPayload(mensaje) // Usa .WithPayloadSegment(Encoding.UTF8.GetBytes(mensaje)) si usas MQTTnet v4+ y te marca advertencia
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        }
    }
}
