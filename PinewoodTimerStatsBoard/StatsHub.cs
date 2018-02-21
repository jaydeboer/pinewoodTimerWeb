using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace PinewoodTimerStatsBoard
{
    public class StatsHub : Hub
    {
        public Task NewTrack(int trackNumber)
        {
            Console.WriteLine($"New Track {trackNumber}");
            return Clients.All.InvokeAsync("NewTrack", trackNumber);
        }
        public Task StartRace(int trackNumber)
        {
            Console.WriteLine("Race Started");
            return Clients.All.InvokeAsync("StartRace", trackNumber);
        }
        public Task LaneFinished(int trackNumber, int lane, int place, string time)
        {
            Console.WriteLine($"Track {trackNumber} lane {lane} in place {place} with time {time}");
            return Clients.All.InvokeAsync("LaneFinished", trackNumber, lane, place, time);
        }
    }
}
