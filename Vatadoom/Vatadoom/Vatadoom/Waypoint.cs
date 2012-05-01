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
        public BoundingBox BoundingRectangle { get; set; }
        public Vector2 TileCoords { get; set; }
        public WaypointType Type { get; private set; }
        
        public Waypoint(WaypointType type, WaypointHandler h, Vector2 tileCoords, float depth)
        {
            Type = type;
            handler = h;
            TileCoords = tileCoords;
            if (type == WaypointType.Lift)
                BoundingRectangle = new BoundingBox(new Vector3(tileCoords.X, tileCoords.Y, depth), new Vector3(tileCoords.X + 300, tileCoords.Y + 40, depth));
            else
                BoundingRectangle = new BoundingBox(new Vector3(tileCoords.X, tileCoords.Y, depth), new Vector3(tileCoords.X + 60, tileCoords.Y + 40, depth));
        }

        public void handleEvent()
        {
            handler.handleEvent(this);
        }
    }
}
