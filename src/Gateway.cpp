#include <Arduino.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <ArduinoJson.h>
#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"
#include "esp_private/esp_clk.h"

#define ANCHO_PANTALLA 128
#define ALTO_PANTALLA 64
Adafruit_SSD1306 oled(ANCHO_PANTALLA, ALTO_PANTALLA, &Wire, -1);

#define RXD2 16
#define TXD2 17
#define PIN_AUX 4
#define PIN_M1 18
#define PIN_M0 19 
#define LED_AZUL 2

const char* ssid = "OPPO";
const char* password = "123456789";
const char* mqtt_server = "servidorhall.sytes.net";
const int mqtt_port = 1883;
const char* mqtt_user = "albertoll06";
const char* mqtt_pass = "&hall$2021#";

// TÓPICOS
const char* abrir_topic = "UPT/LABORATORIOS"; // Escucha la app de escritorio
const char* status_topic = "UPT/LABORATORIOS/doorStatus"; // Raíz unificada para el carril único

WiFiClient espClient;
PubSubClient client(espClient);

unsigned long ultimoIntentoMQTT = 0;
const unsigned long intervaloReconexion = 5000;

void esperarAUX() {
  while (digitalRead(PIN_AUX) == LOW) { delay(1); }
  delay(5);
}

// ESCUCHA LA APP DE ESCRITORIO Y RETRANSMITE AL AIRE
void callback(char* topic, byte* payload, unsigned int length) {
  JsonDocument doc;
  DeserializationError error = deserializeJson(doc, payload, length);
  if (error) return;

  int direccionDestino = doc["d"];      
  const char* comando = doc["c"];        

  if (comando != NULL && strcmp(comando, "abrir") == 0) {
    Serial.print("[MQTT MULTIPUNTO] Redirigiendo orden al aire hacia el Nodo: ");
    Serial.println(direccionDestino);

    esperarAUX();
    Serial2.write(0x00); 
    Serial2.write((byte)direccionDestino); 
    Serial2.write(0x35); 
    Serial2.println(comando); 
    esperarAUX();
  }
}

void setupWiFi() {
  delay(10);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) { delay(500); }
  oled.clearDisplay(); oled.setCursor(0,0); oled.setTextSize(1);
  oled.setTextColor(SSD1306_WHITE); oled.println("WiFi: OK!"); oled.display();
}

void reconectarMQTT() {
  unsigned long tiempoActual = millis();
  if (tiempoActual - ultimoIntentoMQTT >= intervaloReconexion) {
    ultimoIntentoMQTT = tiempoActual;
    Serial.print("Intentando conexion MQTT...");
    
    if (client.connect("ESP32_Gateway", mqtt_user, mqtt_pass, "UPT/LABORATORIOS/status", 1, true, "offline")) {
      Serial.println(" ¡Conectado con Exito!");
      oled.println("MQTT: OK!"); oled.display();
      client.publish("UPT/LABORATORIOS/status", "online", true);
      client.subscribe(abrir_topic); 
    } else {
      Serial.println(" Fallo, codigo: " + String(client.state()));
    }
  }
}

void setup() {
  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0); 
  
  Serial.begin(115200);                        
  Serial2.begin(9600, SERIAL_8N1, RXD2, TXD2); 
  
  pinMode(PIN_M0, OUTPUT); pinMode(PIN_M1, OUTPUT); pinMode(PIN_AUX, INPUT);
  pinMode(LED_AZUL, OUTPUT); digitalWrite(LED_AZUL, LOW);
  esperarAUX();

  digitalWrite(PIN_M0, HIGH); digitalWrite(PIN_M1, HIGH);
  esperarAUX();
  
  byte conf[] = {0xC0, 0x00, 0x01, 0x1A, 0x35, 0xC0};
  Serial2.write(conf, 6);
  Serial2.flush();
  esperarAUX();
  
  digitalWrite(PIN_M0, LOW); digitalWrite(PIN_M1, LOW);
  delay(200); 

  Serial2.flush();
  while(Serial2.available() > 0) { Serial2.read(); }

  if(!oled.begin(SSD1306_SWITCHCAPVCC, 0x3C)) { Serial.println("Error OLED"); }
  oled.clearDisplay(); oled.setTextSize(1); oled.setTextColor(SSD1306_WHITE);
  oled.setCursor(0,0); oled.println("GATEWAY APP READY"); oled.display();

  setupWiFi();
  client.setServer(mqtt_server, mqtt_port);
  client.setCallback(callback);
}

void loop() {
  if (WiFi.status() != WL_CONNECTED) setupWiFi();
  if (!client.connected()) reconectarMQTT();
  else client.loop();

  // --- ESCUCHA RESPUESTAS DESDE EL TRANSMISOR ---
  if (Serial2.available() > 0) {
    String resp = Serial2.readStringUntil('\n');
    resp.trim(); 

    if (resp.length() > 2 && resp.indexOf("LAB") != -1) { 
      digitalWrite(LED_AZUL, HIGH); 
      
      int posicionSeparador = resp.indexOf('|');
      if (posicionSeparador != -1) {
        String sensor = resp.substring(0, posicionSeparador); // Ej: "LAB04" o "LAB:04_S1"
        String estado = resp.substring(posicionSeparador + 1); // Ej: "abierta"
        
        estado.toLowerCase(); 

        oled.clearDisplay(); oled.setCursor(0, 0); oled.setTextSize(1);
        oled.println("--- MONITOR CLOUD ---"); oled.println(""); oled.setTextSize(1);
        oled.print("Sensor: "); oled.println(sensor); oled.setTextSize(2);
        oled.print("E: "); oled.println(estado); oled.display();

        // PUBLICACIÓN DE CARRIL ÚNICO
        if (client.connected()) {
          // Construimos el tópico dinámico (Ej: UPT/LABORATORIOS/doortopic/LAB04)
          String topicoDinamico = String(status_topic) + "/" + sensor;
          
          Serial.print("[MQTT APP] Publicando en: ");
          Serial.println(topicoDinamico);
          
          // Enviamos únicamente "abierta" o "cerrada" a esa ruta
          client.publish(topicoDinamico.c_str(), estado.c_str()); 
        }
      }
      delay(150);
      digitalWrite(LED_AZUL, LOW);
    }
  }
}