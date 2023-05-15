using nanoFramework.Json;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.Networking;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Mqtt
{
    public class Program
    {
        const int BusId = 1;
        const int I2CClockPin = 22;
        const int I2CDataPin = 21;

        // Verbindungsdaten für das WiFi Netzwerk
        const string Ssid = "Meine SSID";
        const string Password = "Sehr geheimes Passwort";
        const string MqttServer = "192.168.123.123";

        private static GpioController _gpioController;
        private static GpioPin _led;

        public static void Main()
        {
            // LED konfigurieren
            _gpioController = new GpioController();
            _led = _gpioController.OpenPin(2, PinMode.Output);
            _led.Write(PinValue.Low);

            // WLAN Verbindung aufbauen
            Debug.WriteLine("Verbindung mit WLAN Netzwerk herstellen...");

            // Max. 60 Sekunden auf eine WLAN Verbindung warten
            CancellationTokenSource cs = new(60000);
            var success = WifiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true, token: cs.Token);
            if (!success)
            {
                // Irgendetwas ist schief gelaufen: Fehlermeldung ausgeben
                Debug.WriteLine($"Verbindung zum WLAN Netzwerk nicht möglich: {WifiNetworkHelper.Status}");
                if (NetworkHelper.HelperException != null)
                {
                    Debug.WriteLine($"ex: {NetworkHelper.HelperException}");
                }

                Thread.Sleep(Timeout.Infinite);
            }

            // Verbindung zum Mqtt Server herstellen
            var mqtt = new MqttClient(MqttServer, 1883, false, null, null, MqttSslProtocols.None);
            mqtt.ProtocolVersion = MqttProtocolVersion.Version_5;
            var ret = mqtt.Connect("esp32Weather", true);
            if (ret != MqttReasonCode.Success)
            {
                Debug.WriteLine($"Verbindungsaufbau zum MQTT Server nicht möglich: {ret}");
                mqtt.Disconnect();
                Thread.Sleep(Timeout.Infinite);
                return;
            }
            Debug.WriteLine("Verbindung zum MQTT Server erfolgreich aufgebaut.");

            // Event registrieren und auf ein Topic lauschen
            mqtt.MqttMsgPublishReceived += MqttMessageReceived;
            mqtt.Subscribe(new[] { "netframework/led" }, new[] { MqttQoSLevel.AtLeastOnce });

            using (Bme280Sensor bme280 = new Bme280Sensor())
            {
                bme280.Init(BusId, I2CDataPin, I2CClockPin);

                while (true)
                {
                    var sensorData = bme280.ReadJsonValues();
                    mqtt.Publish("netframework/test", Encoding.UTF8.GetBytes(sensorData));

                    Thread.Sleep(1000);
                }
            }
        }

        private static void MqttMessageReceived(object sender, MqttMsgPublishEventArgs eventArgs)
        {
            var message = Encoding.UTF8.GetString(eventArgs.Message, 0, eventArgs.Message.Length);
            Debug.WriteLine(String.Format("Nachricht auf topic '{0}' mit dem Wert {1} empfangen.", eventArgs.Topic, message));

            // JSON in Objekt umwandeln
            var ledMessage = JsonConvert.DeserializeObject(message, typeof(LedMessage)) as LedMessage;
            if (ledMessage != null && ledMessage.Enabled)
            {
                Debug.WriteLine("LED einschalten.");
                _led.Write(PinValue.High);
            } 
            else 
            {
                Debug.WriteLine("LED ausschalten.");
                _led.Write(PinValue.Low);
            }
        }
    }
}
