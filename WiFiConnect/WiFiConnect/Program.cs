using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace WiFiConnect
{
    public class Program
    {
        // Verbindungsdaten für das WiFi Netzwerk
        const string Ssid = "Meine SSID";
        const string Password = "Sehr geheimes Passwort";

        public static void Main()
        {
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
            }

            while (true)
            {
                // Verbindung zum Netzwerk erfolgreich: IPv4 Adresse ausgeben
                Debug.WriteLine("WLAN Verbindung erfolgreich aufgebaut.");
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        Debug.WriteLine($"IPv4 Adresse: {ni.IPv4Address}");
                    }
                }

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
