using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace PinewoodTimerStatsBoard
{
    public class StatsHub : Hub
    {
        public Task StartRace()
        {
            Console.WriteLine("Race Started");
            return Clients.All.InvokeAsync("StartRace");
        }
        public Task LaneFinished(int lane, int place, string time)
        {
            Console.WriteLine($"Lane {lane} in place {place} with time {time}");
            return Clients.All.InvokeAsync("LaneFinished", lane, place, time);
        }
    }
}
