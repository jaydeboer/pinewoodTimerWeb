using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Timers;

namespace PineoodTimerHost
{
    class Program
    {
        static void Main(string[] args)
        {
            hub = new HubConnectionBuilder()
                .WithUrl("http://localhost:54856/statshub")
                .Build();
            hub.StartAsync().Wait();

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                try
                {
                    SerialPort mySerialPort = new SerialPort(port);
                    mySerialPort.BaudRate = 115200;
                    mySerialPort.Parity = Parity.None;
                    mySerialPort.StopBits = StopBits.One;
                    mySerialPort.DataBits = 8;
                    mySerialPort.Handshake = Handshake.None;
                    mySerialPort.RtsEnable = false;
                    mySerialPort.DataReceived += MySerialPort_DataReceived;
                    mySerialPort.Open();
                    Console.WriteLine($"Opened port {port}");
                }
                catch
                {
                    Console.WriteLine("No ports found");
                }
            }
            char pressed;
            do
            {
                pressed = Console.ReadKey().KeyChar;
                if (pressed != 'e')
                    ProcessChar(pressed);
            }  while (pressed != 'e');
        }

        private static Stopwatch raceTimer = new Stopwatch();
        private static Timer timeout = new Timer(10000);
        private static List<char> finishedLanes = new List<char>();
        private static HubConnection hub;

        private static void MySerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string inData = sp.ReadExisting();
            foreach (char data in inData)
                ProcessChar(data);
        }

        static void ProcessChar(char data)
        {
            Console.WriteLine($"Received {data}.");
            TimeSpan ts = raceTimer.Elapsed;
            string time = ((double)ts.Seconds + (((double)ts.Milliseconds) / 1000)).ToString("N3");
            switch (data)
            {
                case 's':
                case 'S': // Race started
                    raceTimer.Reset();
                    raceTimer.Start();
                    finishedLanes.Clear();
                    timeout.Enabled = true;
                    hub.InvokeAsync("StartRace");

                    break;
                case '1': // Lane 1 finished.
                case '2': // Lane 2 finished
                case '3': // Lane 3 finished
                case '4': // Lane 4 finished
                    finishedLanes.Add(data);
                    hub.InvokeAsync("LaneFinished", data, finishedLanes.Count, time);
                    Console.WriteLine($"Lane {data} finished with rank {finishedLanes.Count + 1} with an ET of {time} seconds");
                    break;
                default: break;
            }
        }
    }
}
