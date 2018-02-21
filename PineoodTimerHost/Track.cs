using System;
using System.Collections.Generic;
using System.Text;

namespace PineoodTimerHost
{
    class Track
    {
        public Track()
        {
            _Lanes = new Lane[MaxLanes];
        }
        public Lane[] GetPages()
        {
            return (Lane[])_Lanes.Clone();
        }

        const int MaxLanes = 4;
        public Int32 TrackNumber { get; set; }
        public String PortName { get; set; }
        private Lane[] _Lanes;
        public HashSet<char> FinishedLanes = new HashSet<char>();

    }
}
