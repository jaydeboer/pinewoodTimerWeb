using System.Linq;
using System;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using ZXing;

namespace PinewoodTimerStatsBoard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var myIp = GetLocalIPv4(NetworkInterfaceType.Ethernet);
            if (string.IsNullOrWhiteSpace(myIp))
                myIp = GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            else if (string.IsNullOrWhiteSpace(myIp))
                myIp = "127.0.0.1";
            Console.WriteLine(myIp);

            var w = new ZXing.BarcodeWriterSvg();
            w.Format = BarcodeFormat.QR_CODE;
            var image = w.Write($"http://{myIp}:54856");
            
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(".","wwwroot", "images"));
            System.IO.File.WriteAllText(System.IO.Path.Combine(".","wwwroot", "images", "barcode.svg"), image.Content);
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:54856")
                .UseStartup<Startup>()
                .Build();
        
        internal static string GetLocalIPv4(NetworkInterfaceType type)
        {
            string output = string.Empty;
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == type && item.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();

                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                    {
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                output = ip.Address.ToString();
                            }
                        }
                    }
                }
            }

            return output;
        }
    }
}
