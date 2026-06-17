#include <Arduino.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"

#define ANCHO_PANTALLA 128
#define ALTO_PANTALLA 64
Adafruit_SSD1306 oled(ANCHO_PANTALLA, ALTO_PANTALLA, &Wire, -1);

#define RXD2 16
#define TXD2 17
#define PIN_AUX 4
#define PIN_M1 18
#define PIN_M0 19
#define LED_AZUL 2

#define S1 12

int lastS1 = -1;
unsigned long ultimoReporteCiclico = 0;
const unsigned long intervaloReporte = 15000; 

void esperarAUX() {
  while (digitalRead(PIN_AUX) == LOW) { delay(1); }
  delay(5);
}

void enviarMensajeLora(String mensaje) {
  esperarAUX();
  Serial2.write(0x00); 
  Serial2.write(0x01); // Destino: Gateway (Dirección 1)
  Serial2.write(0x35); // Canal 53
  Serial2.println(mensaje); 
  esperarAUX();
}

void setup() {
  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0); 
  
  Serial.begin(115200);
  Serial2.begin(9600, SERIAL_8N1, RXD2, TXD2); 
  
  pinMode(PIN_M0, OUTPUT); pinMode(PIN_M1, OUTPUT); pinMode(PIN_AUX, INPUT);
  pinMode(LED_AZUL, OUTPUT); digitalWrite(LED_AZUL, LOW);
  pinMode(S1, INPUT_PULLUP);

  esperarAUX();
  digitalWrite(PIN_M0, HIGH); digitalWrite(PIN_M1, HIGH);
  esperarAUX();

  byte configuracion[] = {0xC0, 0x00, 0x02, 0x1A, 0x35, 0xC0};
  Serial2.write(configuracion, 6);
  esperarAUX(); 

  digitalWrite(PIN_M0, LOW); digitalWrite(PIN_M1, LOW);
  esperarAUX();
  
  if(!oled.begin(SSD1306_SWITCHCAPVCC, 0x3C)) { Serial.println("Error OLED"); }
  oled.clearDisplay(); oled.setTextSize(1); oled.setTextColor(SSD1306_WHITE);
  oled.setCursor(0, 0); oled.println("--- LAB 02 ---"); oled.println("Esperando app..."); oled.display();
}

void loop() {
  unsigned long tiempoActual = millis();

  // --- 1. ESCUCHA ORDENES DE APERTURA DESDE EL GATEWAY ---
  if (Serial2.available() > 0) {
    String comandoRecibido = Serial2.readStringUntil('\n');
    comandoRecibido.trim();

    if (comandoRecibido.length() > 0) {
      Serial.println("[LORA] Comando desde Gateway: " + comandoRecibido);
      
      if (comandoRecibido == "abrir") {
        digitalWrite(LED_AZUL, HIGH);

        oled.clearDisplay(); oled.setCursor(0, 0); oled.setTextSize(1);
        oled.println("--- SISTEMA ACCIONADO ---"); oled.println(""); oled.setTextSize(2);
        oled.println("ABRIENDO..."); oled.display();

        // Reporte de regreso que el Gateway capturará con éxito porque contiene "LAB"
        enviarMensajeLora("LAB:02_S1|ABRIENDO PUERTA");

        delay(5000); 
        digitalWrite(LED_AZUL, LOW);
        
        lastS1 = -1; 
      }
    }
  }

  // --- 2. MONITOREO INMEDIATO DEL SENSOR 1 ---
  int val1 = digitalRead(S1);
  if (val1 != lastS1) {
    delay(50);
    if (digitalRead(S1) == val1) {
      String estado1 = (val1 == HIGH) ? "ABIERTA" : "CERRADA";
      
      // CORRECCIÓN DE ORO: Añadida la etiqueta LAB obligatoria para pasar el filtro del receptor
      enviarMensajeLora("LAB:02_S1|" + estado1);

      oled.clearDisplay(); oled.setCursor(0, 0); oled.setTextSize(1);
      oled.println("--- LAB 02 ---"); oled.println(""); oled.setTextSize(2);
      oled.print("S1:"); oled.println(estado1); oled.display();
      
      lastS1 = val1;
      ultimoReporteCiclico = tiempoActual;
    }
  }

  // --- 3. REPORTE CÍCLICO CADA 15 SEGUNDOS ---
  if (tiempoActual - ultimoReporteCiclico >= intervaloReporte) {
    ultimoReporteCiclico = tiempoActual; 
    String estadoActualS1 = (digitalRead(S1) == HIGH) ? "ABIERTA" : "CERRADA";
    enviarMensajeLora("LAB:02_S1" + estadoActualS1);
  }
}
