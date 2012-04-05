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
    class Player : WaypointHandler
    {
        public Rectangle TopBoundingRectangle;
        public Rectangle BottomBoundingRectangle;
        private Texture2D texture;
        public Physics Physics;
        private Level currentLevel;
        private float jumpSpeed = 200.0f;
        private float moveSpeed = 200.0f;
        bool canClimb = false;
        bool climbing = false;
        public bool ridingVehicle = false;
        public Player(Game game, Vector2 pos, Level level)
        {
            texture = game.Content.Load<Texture2D>("Player/player");
            TopBoundingRectangle = new Rectangle((int)pos.X, (int)pos.Y, 60, 40);
            BottomBoundingRectangle = new Rectangle((int)pos.X, (int)pos.Y + 40, 60, 40);
            Physics = new Physics();
            Physics.Velocity = jumpSpeed;
            currentLevel = level;
        }

        /// <summary>
        /// Resets the player location to the last saved spawn point
        /// </summary>
        /// <param name="spawn">Point specifying spawn position in level grid</param>
        public void resetRectangle(Point spawn)
        {
            TopBoundingRectangle.Location = new Point(spawn.X * 60, spawn.Y * 40);
            BottomBoundingRectangle.Location = new Point(spawn.X * 60, spawn.Y * 40 + 40);
            Physics.Velocity = jumpSpeed;
            canClimb = false;
            climbing = false;
            ridingVehicle = false;
        }

        public void Update(GameTime gameTime)
        {
            // if riding a vehicle, the vehicle controls the movement. Collision detection remains unchanged however
            if (!ridingVehicle)
            {
                // move right
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    TopBoundingRectangle.Offset((int)Physics.horizontalMotion(TopBoundingRectangle.Center, moveSpeed, gameTime).X, 0);
                    BottomBoundingRectangle.Offset((int)Physics.horizontalMotion(BottomBoundingRectangle.Center, moveSpeed, gameTime).X, 0);
                }

                // move left
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    TopBoundingRectangle.Offset((int)Physics.horizontalMotion(TopBoundingRectangle.Center, -moveSpeed, gameTime).X, 0);
                    BottomBoundingRectangle.Offset((int)Physics.horizontalMotion(BottomBoundingRectangle.Center, -moveSpeed, gameTime).X, 0);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    if (canClimb)
                    {
                        climbing = true;
                        TopBoundingRectangle.Offset(0, -(int)Physics.staticVerticalMotion(TopBoundingRectangle.Center, moveSpeed, gameTime).Y);
                        BottomBoundingRectangle.Offset(0, -(int)Physics.staticVerticalMotion(BottomBoundingRectangle.Center, moveSpeed, gameTime).Y);
                    }
                }

                if (Keyboard.GetState().IsKeyUp(Keys.W))
                {
                    climbing = false;
                }

                // jump under the effects of gravity
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    TopBoundingRectangle.Offset(0, -((int)Physics.dynamicVerticalMotion(TopBoundingRectangle.Center, Physics.Velocity, gameTime).Y));
                    BottomBoundingRectangle.Offset(0, -((int)Physics.dynamicVerticalMotion(BottomBoundingRectangle.Center, Physics.Velocity, gameTime).Y));
                }

                // simulate gravity
                if (Keyboard.GetState().IsKeyUp(Keys.Space))
                {
                    if (!climbing)
                    {
                        if (Physics.Velocity > 0.0f)
                            Physics.Velocity = 0.0f;
                        TopBoundingRectangle.Offset(0, -((int)Physics.dynamicVerticalMotion(TopBoundingRectangle.Center, Physics.Velocity, gameTime).Y));
                        BottomBoundingRectangle.Offset(0, -((int)Physics.dynamicVerticalMotion(BottomBoundingRectangle.Center, Physics.Velocity, gameTime).Y));
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Rectangle.Union(TopBoundingRectangle, BottomBoundingRectangle), Color.White);
        }

        /// <summary>
        /// Tests if the player is colliding with the supplied block
        /// </summary>
        /// <param name="tile">The block to test for collisions</param>
        /// <param name="side">The side where the collision is meant to occur</param>
        /// <param name="gameTime">The current game time. Used for arresting motion, or for calculating gravitational effects when the player is not colliding with a block below them</param>
        public void testCollisions(Tile tile, int side, GameTime gameTime)
        {
            if (TopBoundingRectangle.Intersects(tile.BoundingRectangle))
            {
                // collision detected, process it
                if (tile.collisionType == Tile.CollisionType.Solid)
                {
                    if (tile.tileType == Tile.TileType.Waypoint)
                    {
                        for (int i = 0; i < currentLevel.waypoints.Count; i++)
                            currentLevel.waypoints.ElementAt(i).Value.handleEvent();
                    }

                    else if (tile.tileType == Tile.TileType.Ladder)
                    {
                        canClimb = true;
                    }
                        // else if (tile.tileType == Tile.TileType.Bullet)
                        // damage the player

                    else
                    {
                        if (canClimb)
                            canClimb = !canClimb;
                        // block movement through the block
                        // colliding with a block to your right
                        if (side == 0)
                        {
                            TopBoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, TopBoundingRectangle.Location.Y);
                            BottomBoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, BottomBoundingRectangle.Location.Y);
                        }

                        // colliding with a block to your left
                        if (side == 1)
                        {
                            TopBoundingRectangle.Location = new Point(tile.BoundingRectangle.Right, TopBoundingRectangle.Location.Y);
                            BottomBoundingRectangle.Location = new Point(tile.BoundingRectangle.Right, BottomBoundingRectangle.Location.Y);
                        }

                        // hitting a solid block from beneath, so block movement
                        if (side == 2)
                        {
                            TopBoundingRectangle.Location = new Point(TopBoundingRectangle.Location.X, tile.BoundingRectangle.Bottom);
                            BottomBoundingRectangle.Location = new Point(BottomBoundingRectangle.Location.X, tile.BoundingRectangle.Bottom - 40);
                            if (Physics.Velocity > 0.0f)
                                Physics.Velocity = 0.0f;
                        }
                    }
                }
                // otherwise, the block is passable, so do not test any collisions for it
                else
                {
                    climbing = false;
                    canClimb = false;
                }

            }

            if (BottomBoundingRectangle.Intersects(tile.BoundingRectangle))
            {
                // collision detected, process it
                if (tile.collisionType == Tile.CollisionType.Solid)
                {
                    if (tile.tileType == Tile.TileType.Waypoint)
                    {
                        for (int i = 0; i < currentLevel.waypoints.Count; i++)
                            currentLevel.waypoints.ElementAt(i).Value.handleEvent();
                    }

                    else if (tile.tileType == Tile.TileType.Ladder)
                    {
                        canClimb = true;
                    }
                    // else if (tile.tileType == Tile.TileType.Bullet)
                    // damage the player

                    else
                    {
                        if (canClimb)
                            canClimb = !canClimb;
                        // block movement through the block
                        // colliding with a block to your right
                        if (side == 0)
                        {
                            BottomBoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, BottomBoundingRectangle.Location.Y);
                            TopBoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, TopBoundingRectangle.Location.Y);
                        }
                        // colliding with a block to your left
                        if (side == 1)
                        {
                            BottomBoundingRectangle.Location = new Point(tile.BoundingRectangle.Right, BottomBoundingRectangle.Location.Y);
                            TopBoundingRectangle.Location = new Point(tile.BoundingRectangle.Right, TopBoundingRectangle.Location.Y);
                        }
                        // akin to landing on top of a solid block or platform
                        if (side == 3 && !ridingVehicle)
                        {
                            BottomBoundingRectangle.Location = new Point(BottomBoundingRectangle.Location.X, tile.BoundingRectangle.Top - 40);
                            TopBoundingRectangle.Location = new Point(TopBoundingRectangle.Location.X, tile.BoundingRectangle.Top - 80);
                            Physics.Velocity = jumpSpeed;
                        }
                    }
                }
                else if (tile.collisionType == Tile.CollisionType.Platform)
                {
                    // akin to landing on top of a platform block. No other collisions need to be detected
                    if (side == 3 && !ridingVehicle)
                    {
                        BottomBoundingRectangle.Offset(0, -(BottomBoundingRectangle.Bottom - tile.BoundingRectangle.Top));
                        TopBoundingRectangle.Offset(0, -(TopBoundingRectangle.Bottom - tile.BoundingRectangle.Top));
                        Physics.Velocity = jumpSpeed;
                    }
                }
                // otherwise, the block is passable, so do not test any collisions for it
                else
                {
                    climbing = false;
                    canClimb = false;
                }

            }
        }
        public void handleEvent(Waypoint w)
        {
            // begin riding vehicle

            if (w.TileCoords.X == BottomBoundingRectangle.Right / 60 && w.TileCoords.Y == BottomBoundingRectangle.Bottom / 40 - 1)
            {
                // begin riding vehicle
                if (w.Type == Waypoint.WaypointType.Spinner)
                {
                    ridingVehicle = true;
                    TopBoundingRectangle.Location = new Point(w.TileCoords.X * 60 - 60, w.TileCoords.Y * 40 - 80);
                    BottomBoundingRectangle.Location = new Point(w.TileCoords.X * 60 - 60, w.TileCoords.Y * 40 - 40);
                }

                // end riding vehicle
                else if (w.Type == Waypoint.WaypointType.EndRide)
                {
                    ridingVehicle = false;
                    TopBoundingRectangle.Location = new Point(w.TileCoords.X * 60 - 60, w.TileCoords.Y * 40 - 80);
                    BottomBoundingRectangle.Location = new Point(w.TileCoords.X * 60 - 60, w.TileCoords.Y * 40 - 40);
                }
            }
        }
    }
}
