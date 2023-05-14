using nanoFramework.Networking;
using System.Device.Gpio;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace HttpRequest
{
    public class Program
    {
        const int BusId = 1;
        const int I2CClockPin = 22;
        const int I2CDataPin = 21;

        // Verbindungsdaten für das WiFi Netzwerk
        const string Ssid = "Meine SSID";
        const string Password = "Sehr geheimes Passwort";

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

            Debug.WriteLine("WLAN Verbindung erfolgreich hergestellt.");

            using (Bme280Sensor bme280 = new Bme280Sensor())
            {
                bme280.Init(BusId, I2CDataPin, I2CClockPin);

                using (var httpClient = new HttpClient())
                {
                    // SSL Verschlüsselung aktivieren
                    httpClient.SslProtocols = SslProtocols.Tls12;
                    var certificateBytes = Resources.GetBytes(Resources.BinaryResources.sca1b);
                    httpClient.HttpsAuthentCert = new X509Certificate(certificateBytes);

                    while (true)
                    {
                        var sensorData = bme280.ReadJsonValues();
                        var stringContent = new StringContent(sensorData, Encoding.UTF8, "application/json");

                        var response = httpClient.Post("https://httpbin.org/post", stringContent);
                        Debug.WriteLine(response.Content.ReadAsString());

                        Thread.Sleep(10000);
                    }
                }
            }
        }
    }
}
