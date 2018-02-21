﻿using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;

namespace PineoodTimerHost
{
    class Program
    {
        static void Main(string[] args)
        {
            portWatcher.Elapsed += portWatcher_Elapsed;
            signalrConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:54856/statshub")
                .Build();
            signalrConnection.StartAsync().Wait();

            portWatcher_Elapsed(null, null);
            if (!knownPorts.Any())
            {
                AddTrack("COM1");
            }
            selectedPort = knownPorts[0];

            PrintInstructions();

            char pressed;
            do
            {
                pressed = Console.ReadKey(true).KeyChar;
                if (pressed != 'e')
                    ProcessChar(pressed, tracks.First(t => t.PortName.Equals(selectedPort, StringComparison.InvariantCultureIgnoreCase)));
            } while (pressed != 'e');
        }

        private static void PrintInstructions()
        {
            Console.WriteLine("The commands are:");
            Console.WriteLine("=================");
            Console.WriteLine("'s' - start new race");
            Console.WriteLine("'1 - 4' - number of the lane to finish");
            Console.WriteLine("'p' - create a new port");
            Console.WriteLine("'t' - toggle through the ports");
            Console.WriteLine("'e' - exit");
        }

        private static void portWatcher_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var ports = SerialPort.GetPortNames().Where(portName => !knownPorts.Contains(portName));
                foreach (var port in ports)
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
                    AddTrack(port);
                }
            }
            catch
            {
            }
        }
        private static List<Track> tracks = new List<Track>();
        private static Stopwatch raceTimer = new Stopwatch();
        private static Timer timeout = new Timer(10000);
        private static Timer portWatcher = new Timer(10000);
        private static List<string> knownPorts = new List<string>();
        private static HubConnection signalrConnection;
        private static string selectedPort;

        private static void AddTrack(string portName)
        {
            knownPorts.Add(portName);
            var track = new Track { PortName = portName, TrackNumber = knownPorts.Count };
            tracks.Add(track);
            signalrConnection.InvokeAsync("NewTrack", track.TrackNumber);
        }

        private static void MySerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string inData = sp.ReadExisting();
            foreach (char data in inData)
                ProcessChar(data, tracks.First(t => t.PortName.Equals(selectedPort, StringComparison.InvariantCultureIgnoreCase)));
        }

        static void ProcessChar(char data, Track track)
        {
            //Console.WriteLine($"Received {data}.");
            TimeSpan ts = raceTimer.Elapsed;
            string time = ((double)ts.Seconds + (((double)ts.Milliseconds) / 1000)).ToString("N3");
            switch (data)
            {
                case 's':
                case 'S': // Race started
                    raceTimer.Reset();
                    raceTimer.Start();
                    track.FinishedLanes.Clear();
                    timeout.Enabled = true;
                    signalrConnection.InvokeAsync("StartRace", track.TrackNumber);

                    break;
                case '1': // Lane 1 finished.
                case '2': // Lane 2 finished
                case '3': // Lane 3 finished
                case '4': // Lane 4 finished
                    var previousMax = track.FinishedLanes.Count;
                    track.FinishedLanes.Add(data);
                    if (previousMax != track.FinishedLanes.Count)
                    {
                        signalrConnection.InvokeAsync("LaneFinished", track.TrackNumber, data, track.FinishedLanes.Count, time);
                        //Console.WriteLine($"Lane {data} finished with rank {finishedLanes.Count + 1} with an ET of {time} seconds");
                    }
                    break;
                // the rest of these commands are for testing only
                case 'p':
                case 'P':
                    AddTrack($"COM{knownPorts.Count + 1}");
                    Console.WriteLine($"Added port {knownPorts.Last()}");
                    break;
                case 't':
                case 'T':
                    var currentIndex = knownPorts.IndexOf(selectedPort);
                    var newIndex = currentIndex+1;
                    if (newIndex >= knownPorts.Count)
                        newIndex = 0;
                    selectedPort = knownPorts[newIndex];
                    Console.WriteLine($"Selected port {selectedPort}");
                    break;
                default: break;
            }
        }
    }
}
