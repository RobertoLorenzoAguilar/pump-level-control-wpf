# pump-level-control-wpf
Control Nivel GUI

Proyecto que implementa un sistema para monitoreo de nivel y control de una bomba, separando la interfaz de usuario en WPF (Windows Presentation Foundation) bajo el patrón MVVM, y el mundo físico controlado mediante un Arduino conectado por puerto Serial.

## Diagramas del Sistema

En el directorio `docs/` se encuentran los diagramas de la arquitectura documentados como código, utilizando formato [Mermaid](https://mermaid.js.org/).

### 1. Diagrama de Casos de Uso
*(Fuente: `docs/use-case.mmd`)*

```mermaid
flowchart LR
    Operador([Operador])
    Arduino([Arduino Hardware])
    Bomba([Bomba de Agua])

    subgraph WPF [Sistema WPF]
        UC1[Monitorear Nivel de Tanque]
        UC2[Configurar Umbral de Llenado]
        UC3[Control Automático de Bomba]
    end

    Operador --> UC1
    Operador --> UC2
    Arduino -.-> |Envía nivel analógico| UC1
    UC3 -.-> |Envía Comando 1/0| Arduino
    Arduino -.-> |Enciende/Apaga| Bomba
```

### 2. Diagrama de Secuencia 
*(Fuente: `docs/sequence.mmd`)*

```mermaid
sequenceDiagram
    participant Arduino
    participant SerialService as SerialSensorService
    participant ViewModel as MainViewModel
    participant View as MainWindow

    loop Cada 500ms
        Arduino->>SerialService: Envía ADC vía Serial (ej. "450\n")
        SerialService->>SerialService: DataReceived(ADC -> Porcentaje)
        SerialService->>ViewModel: Dispara evento DataReceived(43.9)
        ViewModel->>View: OnPropertyChanged("CurrentLevel")
        ViewModel->>ViewModel: CheckPumpLogic()
        
        alt CurrentLevel < Threshold
            ViewModel->>ViewModel: PumpStatus = "ENCENDIDA"
            ViewModel->>SerialService: Enviar Comando ("1")
            SerialService->>Arduino: Escribe '1' en Puerto Serial
            Arduino->>Arduino: digitalWrite(RELAY_PIN, LOW)
        else CurrentLevel >= Threshold
            ViewModel->>ViewModel: PumpStatus = "Apagada"
            ViewModel->>SerialService: Enviar Comando ("0")
            SerialService->>Arduino: Escribe '0' en Puerto Serial
            Arduino->>Arduino: digitalWrite(RELAY_PIN, HIGH)
        end
        ViewModel->>View: OnPropertyChanged("PumpStatus")
    end
```

### 3. Diagrama de Clases (Arquitectura MVVM)
*(Fuente: `docs/class-diagram.mmd`)*

```mermaid
classDiagram
    class ISensorService {
        <<interface>>
        +event EventHandler~double~ DataReceived
        +GetCurrentLevel() double
    }

    class SerialSensorService {
        -SerialPort _serialPort
        -string _portName
        -int _baudRate
        +SerialSensorService(string, int)
        -SetupSerialPort()
        +Open()
        -SerialPort_DataReceived(object, SerialDataReceivedEventArgs)
        +Close()
        +Dispose()
    }

    class MainViewModel {
        -ISensorService _sensorService
        -double _currentLevel
        -double _threshold
        -string _pumpStatus
        +double CurrentLevel
        +double Threshold
        +string PumpStatus
        +MainViewModel(ISensorService)
        -CheckPumpLogic()
        +event PropertyChangedEventHandler PropertyChanged
        #OnPropertyChanged(string)
    }

    class MainWindow {
        <<View>>
        -MainViewModel _viewModel
    }

    SerialSensorService ..|> ISensorService : Implements
    MainViewModel --> ISensorService : Uso (Inyección/Instanciación)
    MainWindow --> MainViewModel : DataContext
```

## Especificaciones de Hardware Actual (Sensor)

Actualmente, el sistema utiliza un sensor estándar de contacto para comprobar la lectura analógica de porcentaje frente al problema simulado.

- **Fabricante:** Tecneu (Número de parte/Modelo: BAQ75U2)
- **ID Comercial:** ASIN B0BBQ7DTW6
- **Dimensiones:** 36 mm x 9 mm x 10 mm
- **Peso:** 20 g
- **Alimentación:** Corriente Continua (CC), sin necesidad de baterías.
- **Detalles físicos:** Color multicolor.

## Trabajos Futuros (Nice to Have) / Cosas por Hacer

Para lograr que este proyecto transicione satisfactoriamente de un prototipo de escritorio o simulador funcional hacia un sistema de telemetría industrial de gas, deberíamos enfocar el hardware sumando los siguientes elementos:

- [ ] **Sensor IMU (Acelerómetro XYZ y Giroscopio):** Acoplar dispositivos que evalúen la vibración estructural de motores y tuberías. En instrumentación, correlacionar la aceleración con las frecuencias de una bomba y sus tuberías permite evaluar su **estado de salud**, identificando si los rodamientos fallan o existe resonancia y comportamiento anómalo.
- [ ] **Micrófono Ultrasónico:** Dispositivo de monitoreo de sonido en alta frecuencia diseñado específicamente para encontrar y aislar de manera anticipada posibles microfugas en bridas y tuberías de alimentación de gas.
- [ ] **Medición No Invasiva mediante Efecto Hall:** Una medición invasiva (perforar/contacto) en un tanque de gas es peligrosa. La mayoría de estos tanques tienen un reloj/indicador flotante analógico, cuyo flotador interno asciende acoplado a un imán. La propuesta definitiva es usar un **sensor de Efecto Hall** fijo frente al cristal de este indicador. Así, se recoge la fluctuación de este campo magnético (asociada al ascenso del flotador) y el efecto se encarga de convertir de manera pasiva y segura la fuerza del imán en un diferencial de voltaje legible por el Arduino. Mismo resultado, con nulo riesgo de chispa.
