

namespace Shared.Services;
public static class MqttServices
{
    // Datos de conexion MQTT
    public static string host = "localhost";
    public static int port = 8883;

    //Topicos MQTT
    public static string abrir = "casa/cerradura/control";
    public static string peticion = "casa/peticion/control";
}
