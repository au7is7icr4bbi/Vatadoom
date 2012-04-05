﻿using System;
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
            Powerline,
            Ladder
        };

        public enum CollisionType
        {
            Passable, // no collision detection necessary
            Platform, // only detect collisions along the top of the block, and only from above, not from below
            Solid // detect collisions on all sides
        };
        public TileType tileType { get; private set; }
        public CollisionType collisionType { get; private set; }
        public String ID { get; set; }
        private Texture2D texture;
        public Rectangle BoundingRectangle { get; private set; }

        /// <summary>
        /// Create a visible tile. Useful for floors, ceilings and walls (both in background and in entity space)
        /// </summary>
        /// <param name="t">The tile texture, pre-loaded in the level</param>
        /// <param name="position">The tile's world coordinates</param>
        /// <param name="type">The tile type, determined when it is initially read in</param>
        public Tile(Texture2D t, Vector2 position, TileType type)
        {
            texture = t;
            BoundingRectangle = new Rectangle((int)position.X, (int)position.Y, 60, 40);
            tileType = type;
            setCollisions();
        }

        /// <summary>
        /// Create an invisible tile. USeful for the spawn tile or waypoint tiles
        /// </summary>
        /// <param name="position">The tile's world coordinates</param>
        /// <param name="type">The tile type, determined when it is initially read in</param>
        public Tile(Vector2 position, TileType type)
        {
            texture = null;
            BoundingRectangle = new Rectangle((int)position.X, (int)position.Y, 60, 40);
            tileType = type;
            setCollisions();
        }

        /// <summary>
        /// Draw all drawable tiles
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                spriteBatch.Draw(texture, BoundingRectangle, Color.White);
        }

        /// <summary>
        /// Set the collision property based on the tile's type
        /// </summary>
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
                case TileType.Ladder:
                    collisionType = CollisionType.Solid;
                    break;
                case TileType.Powerline:
                    collisionType = CollisionType.Platform;
                    break;
                default:
                    break;
            }
        }
    }
}
