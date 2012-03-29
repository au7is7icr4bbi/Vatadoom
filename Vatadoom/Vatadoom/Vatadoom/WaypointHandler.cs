using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vatadoom
{
    public interface WaypointHandler
    {
        void handleEvent(Waypoint w);
    }
}
