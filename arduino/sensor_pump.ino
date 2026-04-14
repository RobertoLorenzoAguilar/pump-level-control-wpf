// Definición de Pines
const int SENSOR_PIN = A0;    // Sensor de humedad/nivel
const int RELAY_PIN = 7;      // Pin para el módulo de relevador

// Variables de control
unsigned long lastSendTime = 0;
const int sendInterval = 500; // Enviar cada 500ms (evita saturar el puerto)

void setup() {
  Serial.begin(9600);
  pinMode(RELAY_PIN, OUTPUT);
  
  // Por seguridad, iniciamos con la bomba apagada
  // Nota: Muchos relevadores se apagan con HIGH si son Active Low
  digitalWrite(RELAY_PIN, HIGH); 
}

void loop() {
  // 1. LECTURA Y ENVÍO (Mundo Físico -> Digital)
  if (millis() - lastSendTime >= sendInterval) {
    int adcValue = analogRead(SENSOR_PIN);
    
    // Enviamos el valor crudo (0-1023)
    // C# se encargará de la conversión a % (Single Responsibility)
    Serial.println(adcValue); 
    
    lastSendTime = millis();
  }

  // 2. ESCUCHA DE COMANDOS (Digital -> Mundo Físico)
  if (Serial.available() > 0) {
    char command = Serial.read();
    
    if (command == '1') {
      // Encender bomba (Lógica Active Low común en relevadores)
      digitalWrite(RELAY_PIN, LOW); 
    } 
    else if (command == '0') {
      // Apagar bomba
      digitalWrite(RELAY_PIN, HIGH);
    }
  }
}