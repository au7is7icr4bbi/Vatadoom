using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Vatadoom
{
    public class Tile
    {
        public enum TileType
        {
            Road,
            Concrete,
            Player, // player spawn point. Moved when a save point is reached
            BuildingWall,
            BuildingInterior, // background/foreground tile
            SewerWall,
            SewerInterior, // background/foreground tile
            Spinner,
            Platform,
            Air, // nothingness
            Waypoint, // waypoints for triggering special events
            Powerline
        };

        public enum CollisionType
        {
            Passable, // no collision detection necessary
            Platform, // only detect collisions along the top of the block, and only from above, not from below
            Solid // detect collisions on all sides
        };
        public TileType tileType { get; private set; }
        public CollisionType collisionType { get; private set; }
        private Texture2D texture;
        public Rectangle BoundingRectangle { get; private set; }

        // constructor for visible tiles
        public Tile(Game game, Texture2D t, Vector2 position, TileType type)
        {
            texture = t;
            BoundingRectangle = new Rectangle((int)position.X, (int)position.Y, 60, 40);
            tileType = type;
            setCollisions();
        }

        // constructor for invisible tiles
        public Tile(Game game, Vector2 position, TileType type)
        {
            texture = null;
            BoundingRectangle = new Rectangle((int)position.X, (int)position.Y, 60, 40);
            tileType = type;
            setCollisions();
        }

        // draw drawable tiles
        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                spriteBatch.Draw(texture, BoundingRectangle, Color.White);
        }

        // set the collision property based on the block type
        private void setCollisions()
        {
            switch (tileType)
            {
                case TileType.Air:
                    collisionType = CollisionType.Passable;
                    break;
                case TileType.BuildingWall:
                    collisionType = CollisionType.Solid;
                    break;
                case TileType.BuildingInterior:
                    collisionType = CollisionType.Passable;
                    break;
                case TileType.SewerWall:
                    collisionType = CollisionType.Solid;
                    break;
                case TileType.SewerInterior:
                    collisionType = CollisionType.Passable;
                    break;
                case TileType.Player:
                    collisionType = CollisionType.Passable;
                    break;
                case TileType.Waypoint:
                    collisionType = CollisionType.Solid;
                    break;
                case TileType.Platform:
                    collisionType = CollisionType.Platform;
                    break;
                case TileType.Road:
                case TileType.Concrete:
                    collisionType = CollisionType.Solid;
                    break;
                default:
                    break;
            }
        }
    }
}
