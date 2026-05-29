

namespace Shared.Services;
public static class MqttServices
{
    // Datos de conexion MQTT
    public static string host = "servidorhall.sytes.net";
    public static int port = 1883;
    public static string Username = "albertoll06";
    public static string Pasword = "&hall$2021#";


    //Topicos MQTT
    public static string abrir = "UPT/LABORATORIOS";
    public static string peticion = "casa/peticion/control";
    public static string statusTopic = "UPT/LABORATORIOS/status";
    public static string conexionMovil = "peticion/movil/conexion";
    public static string doorTopic = "UPT/LABORATORIOS/doorStatus";
}
