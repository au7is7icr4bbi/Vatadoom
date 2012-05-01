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
    class Transition
    {
        // id is commonly the logical order of transition usage
        public int id { get; private set; } // an integer identifier read in from the tile map. The id is unique to a layer (that is, a 2 transition only occurs once in a layer)
        private int srcx; // indicates the coordinates of the source
        private int srcy;
        private TileLayer[] layers;
        public int currentLayer { get; private set; }

        public Transition(int id, int x, int y, TileLayer[] layers, int layer)
        {
            layers = new TileLayer[3];
            this.id = id;
            this.srcx = x;
            this.srcy = y;
            currentLayer = layer;
        }

        // called when the transition waypoint is touched by the player
        public void movePlayer(Player player)
        {
            for (int i = 0; i < 3; i++)
            {
                if (layers[i].containsTransition(id))
                {
                    // move the player to the new layer, at the location of the transition block
                    player.BoundingRectangle.Min = new Vector3(srcx * 60, srcy * 40, layers[i].depth);
                    player.BoundingRectangle.Max = new Vector3(srcx * 60 + 60, srcy * 40 + 80, layers[i].depth);
                }
            }
        }
    }
}
