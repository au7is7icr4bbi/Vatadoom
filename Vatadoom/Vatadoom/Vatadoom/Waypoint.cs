using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Vatadoom
{
    public class Waypoint
    {
        public enum WaypointType
        {
            SavePoint,
            EndLevel,
            Event,
            Spinner,
            Jeep,
            Minecart,
            Rocket,
            Lift,
            EndRide,
        };

        public WaypointHandler handler;
        public Rectangle BoundingRectangle { get; set; }
        public Point TileCoords { get; set; }
        public WaypointType Type { get; private set; }
        
        public Waypoint(WaypointType type, WaypointHandler h, Point tileCoords)
        {
            Type = type;
            handler = h;
            TileCoords = tileCoords;
            if (type == WaypointType.Lift)
                BoundingRectangle = new Rectangle(tileCoords.X * 60, tileCoords.Y * 40, 300, 40);
            else
                BoundingRectangle = new Rectangle(tileCoords.X * 60, tileCoords.Y * 40, 60, 40);
        }

        public void handleEvent()
        {
            handler.handleEvent(this);
        }
    }
}
