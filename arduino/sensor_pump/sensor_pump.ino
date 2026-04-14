// Definición de Pines y Hardware:
// Sensor: Tecneu BAQ75U2 (Sensor Analógico de Humedad de Suelo YL-69)
// Actuador: Módulo Relevador 1 Canal 5V 10A (LOW LEVEL TRIGGER) + Mini Bomba Sumergible
const int SENSOR_PIN = A0;    // Pin para la lectura analógica de la tierra
const int RELAY_PIN = 7;      // Pin digital para disparar el relevador

// Variables de telemetría
unsigned long lastSendTime = 0;
const int sendInterval = 500; // Enviar información a WPF cada medio segundo (500ms)

void setup() {
  // Abrimos las comunicaciones por USB a 9600 baudios
  Serial.begin(9600);
  pinMode(RELAY_PIN, OUTPUT);
  
  // ¡LÓGICA CRÍTICA DE TU HARDWARE!
  // Como tu módulo es un "LOW LEVEL RELAY", esto quiere decir que funciona con lógica inversa:
  // - digitalWrite LOW = Cierra el circuito (PRENDE LA BOMBA)
  // - digitalWrite HIGH = Abre el circuito (APAGA LA BOMBA)
  // Por lo tanto, al encender el Arduino, iniciamos en HIGH por seguridad para que la bomba no arranque sola.
  digitalWrite(RELAY_PIN, HIGH); 
}

void loop() {
  // 1. LECTURA Y ENVÍO (Del Sensor a la PC)
  if (millis() - lastSendTime >= sendInterval) {
    
    // analogRead mide la resistencia eléctrica de la tierra y nos da de 0 a 1023
    // Nota YL-69: Suelo encharcado (~200 a 400), Suelo seco/polvo (~800 a 1023)
    int adcValue = analogRead(SENSOR_PIN);
    
    // Mandamos el valor crudo al WPF. La interfaz C# hará las matemáticas de porcentajes.
    Serial.println(adcValue); 
    
    lastSendTime = millis();
  }

  // 2. ESCUCHA DE COMANDOS (De la PC al Relevador)
  if (Serial.available() > 0) {
    char command = Serial.read();
    
    if (command == '1') {
      // Activa la bomba enviando CERO VOLTIOS al pin de comando del relevador (Low Trigger)
      digitalWrite(RELAY_PIN, LOW); 
    } 
    else if (command == '0') {
      // Desactiva la bomba enviando 5V al relé
      digitalWrite(RELAY_PIN, HIGH);
    }
  }
}