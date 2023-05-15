using System;
using System.Collections.Generic;
using System.Text;

namespace Mqtt
{
    internal class Bme280SensorMessage
    {
        public Bme280SensorMessage(double temperature, double pressure, double humidity)
        {
            Temperature = temperature;
            Pressure = pressure;
            Humidity = humidity;
        }

        public double Temperature { get; private set; }

        public double Pressure { get; private set; }

        public double Humidity { get; private set; }
    }
}
