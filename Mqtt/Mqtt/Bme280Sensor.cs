using Iot.Device.Bmxx80;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Json;
using System;
using System.Device.I2c;

namespace Mqtt
{
    internal class Bme280Sensor : IDisposable
    {
        private Bme280 bme280;
        private bool disposedValue;

        internal void Init(int busId, int dataPin, int clockPin)
        {
            Configuration.SetPinFunction(dataPin, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(clockPin, DeviceFunction.I2C1_CLOCK);

            // Verbindung zum Sensor konfigurieren
            var i2cSettings = new I2cConnectionSettings(busId, Bme280.SecondaryI2cAddress);
            var i2cDevice = I2cDevice.Create(i2cSettings);
            bme280 = new Bme280(i2cDevice)
            {
                // Sampling der Sensordaten konfigurieren
                TemperatureSampling = Sampling.Standard,
                PressureSampling = Sampling.Standard,
                HumiditySampling = Sampling.Standard,
            };
        }

        internal string ReadJsonValues()
        {
            // Sensordaten abholen
            var sensorData = bme280.Read();

            // Neues Objekt mit den Sensordaten anlegen
            var jsonData = new Bme280SensorMessage(
                sensorData.Temperature.DegreesCelsius,
                sensorData.Pressure.Hectopascals,
                sensorData.Humidity.Percent);

            // Datenklasse in einen JSON String umwandeln
            return JsonConvert.SerializeObject(jsonData);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    bme280.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}