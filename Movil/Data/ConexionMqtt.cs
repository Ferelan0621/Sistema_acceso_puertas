using MQTTnet;
using MQTTnet.Client;
using Shared.Models;
using Shared.Services;

namespace Movil.Data
{
    public class ConexionMqtt
    {   
        
        private IMqttClient mqttClient;
       

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
                    .WithClientId("ClientApp") // ID único
                    .WithTcpServer(MqttServices.host, MqttServices.port)
                    .WithCredentials(MqttServices.Username, MqttServices.Pasword)
                    .WithCleanSession()
                    .Build();

                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {

            }
        }
        //    // 🟢 Evento: Detecta si se conectó con éxito
        //    mqttClient.ConnectedAsync += e =>
        //    {
        //        Console.WriteLine("\n▲ [MQTT] ¡Conectado exitosamente al broker Mosquitto!\n");
        //        return Task.CompletedTask;
        //    };

        //    // 🔴 Evento: Detecta si se desconectó o falló la conexión
        //    mqttClient.DisconnectedAsync += e =>
        //    {
        //        Console.WriteLine($"\n▼ [MQTT] Desconectado del broker. Razón: {e.Reason}\n");
        //        return Task.CompletedTask;
        //    };
        //}
        //public async Task ConectarAsync()
        //{
        //    if (!mqttClient.IsConnected)
        //    {
        //        await mqttClient.ConnectAsync(_options, CancellationToken.None);
        //    }


        //}
        public async Task PublicarMensajeAsync(string topico, string mensaje)
        {
            await ConectarAsync();

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topico)
                .WithPayload(mensaje)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        }
        //JsonAbrir jsonla2 = new JsonAbrir
        //{ d = "3", c = "abrir" };

        //string jsonString = JsonSerializer.Serialize(jsonla2);
        //_mqtt.PublicarMensajeAsync(MqttServices.abrir, jsonString);

        //private async void conectarMqtt()
        //{
        //    try
        //    {
        //        await conexion.ConectarAsync();
        //    }
        //    catch(Exception ex)
        //    {
        //        string texto = "no jalo padrino";
        //        txtUsuario.Text = texto;
        //    }
        //}

    }
}
