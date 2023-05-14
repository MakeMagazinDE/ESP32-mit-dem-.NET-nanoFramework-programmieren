using Iot.Device.Bmxx80;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace BME280
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("BME280 Sensor Test");

            // I2C Pins für Bus 1 auf 21 (SDA) und 22 (SCL) setzen
            // Standard ist Pin 18 (SDA) und Pin 19 (SCL) für Bus 1
            // und Pin 25 (SDA) und Pin 26 (SCL) für Bus 2
            const int busId = 1;
            Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);


            // Verbindung zum Sensor konfigurieren
            var i2cSettings = new I2cConnectionSettings(busId, Bme280.SecondaryI2cAddress);
            using var i2cDevice = I2cDevice.Create(i2cSettings);
            using var bme280 = new Bme280(i2cDevice)
            {
                // Sampling der Sensordaten konfigurieren
                TemperatureSampling = Sampling.Standard,
                PressureSampling = Sampling.Standard,
                HumiditySampling = Sampling.Standard,
            };


            while (true)
            {
                // Messwerte auslesen und ausgeben
                var readResult = bme280.Read();

                Debug.WriteLine(new string('-', 80));
                Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius}\u00B0C");
                Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals}hPa");
                Debug.WriteLine($"Relative humidity: {readResult.Humidity.Percent}%");

                Thread.Sleep(1000);
            }
        }
    }
}
