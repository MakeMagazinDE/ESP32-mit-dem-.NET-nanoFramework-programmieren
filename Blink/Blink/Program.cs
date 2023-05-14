using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace Blink
{
    public class Program
    {
        private static GpioController _gpioController;

        public static void Main()
        {
            Debug.WriteLine("Hello World von .NET!");

            _gpioController = new GpioController();
            GpioPin led = _gpioController.OpenPin(2, PinMode.Output);

            led.Write(PinValue.Low);

            while (true)
            {
                led.Toggle();
                Thread.Sleep(1000);
            }
        }
    }
}
