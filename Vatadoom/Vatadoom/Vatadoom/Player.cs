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
    public class Player : WaypointHandler
    {
        public BoundingBox BoundingRectangle;
        public Rectangle texRect;
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
            BoundingRectangle = new BoundingBox(new Vector3(pos.X, pos.Y, 0.0f), new Vector3(pos.X + texture.Width, pos.Y + texture.Height, 0.0f));
            texRect = new Rectangle((int)pos.X, (int)pos.Y, texture.Width, texture.Height);
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
            BoundingRectangle.Min = new Vector3(spawn.X * 60, spawn.Y * 40, 0.0f);
            BoundingRectangle.Max = new Vector3(BoundingRectangle.Min.X + 60, BoundingRectangle.Min.Y + 80, 0.0f);
            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
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
                    BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X + Physics.horizontalMotion(BoundingRectangle.Min, moveSpeed, gameTime).X, BoundingRectangle.Min.Y, BoundingRectangle.Min.Z);
                    BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X + Physics.horizontalMotion(BoundingRectangle.Max, moveSpeed, gameTime).X, BoundingRectangle.Max.Y, BoundingRectangle.Max.Z);
                    texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                }

                // move left
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X + Physics.horizontalMotion(BoundingRectangle.Min, -moveSpeed, gameTime).X, BoundingRectangle.Min.Y, BoundingRectangle.Min.Z);
                    BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X + Physics.horizontalMotion(BoundingRectangle.Max, -moveSpeed, gameTime).X, BoundingRectangle.Max.Y, BoundingRectangle.Max.Z);
                    texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    if (canClimb)
                    {
                        climbing = true;
                        BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X, BoundingRectangle.Min.Y + -Physics.staticVerticalMotion(BoundingRectangle.Min, moveSpeed, gameTime).Y, BoundingRectangle.Min.Z);
                        BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X, BoundingRectangle.Max.Y + -Physics.staticVerticalMotion(BoundingRectangle.Max, moveSpeed, gameTime).Y, BoundingRectangle.Max.Z);
                        texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                    }
                }

                if (Keyboard.GetState().IsKeyUp(Keys.W))
                {
                    climbing = false;
                }

                // jump under the effects of gravity
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X, BoundingRectangle.Min.Y + -Physics.dynamicVerticalMotion(BoundingRectangle.Min, Physics.Velocity, gameTime).Y, BoundingRectangle.Min.Z);
                    BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X, BoundingRectangle.Min.Y + 80, BoundingRectangle.Max.Z);
                    texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                }

                // simulate gravity
                if (Keyboard.GetState().IsKeyUp(Keys.Space))
                {
                    if (!climbing)
                    {
                        if (Physics.Velocity > 0.0f)
                            Physics.Velocity = 0.0f;
                        BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X, BoundingRectangle.Min.Y + -Physics.dynamicVerticalMotion(BoundingRectangle.Min, Physics.Velocity, gameTime).Y, BoundingRectangle.Min.Z);
                        BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X, BoundingRectangle.Min.Y + 80, BoundingRectangle.Max.Z);
                        texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(BoundingRectangle.Min.X, BoundingRectangle.Min.Y), Color.White);
        }

        /// <summary>
        /// Tests if the player is colliding with the supplied block
        /// </summary>
        /// <param name="tile">The block to test for collisions</param>
        /// <param name="side">The side where the collision is meant to occur</param>
        /// <param name="gameTime">The current game time. Used for arresting motion, or for calculating gravitational effects when the player is not colliding with a block below them</param>
        public void testCollisions(Tile tile, int side, GameTime gameTime)
        {
            if (BoundingRectangle.Intersects(tile.BoundingRectangle))
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
                            BoundingRectangle.Min.X = tile.BoundingRectangle.Min.X - 60;
                            BoundingRectangle.Max.X = tile.BoundingRectangle.Min.X;
                            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                        }

                        // colliding with a block to your left
                        else if (side == 1)
                        {
                            BoundingRectangle.Min.X = tile.BoundingRectangle.Max.X;
                            BoundingRectangle.Max.X = BoundingRectangle.Min.X + 60;
                            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                        }

                        // hitting a solid block from beneath, so block movement
                        else if (side == 2)
                        {
                            BoundingRectangle.Min.Y = tile.BoundingRectangle.Max.Y;
                            BoundingRectangle.Max.Y = BoundingRectangle.Min.Y + 80;
                            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                            if (Physics.Velocity > 0.0f)
                                Physics.Velocity = 0.0f;
                        }

                        else if (side == 3 && !ridingVehicle)
                        {
                            BoundingRectangle.Min.Y = tile.BoundingRectangle.Min.Y - 80;
                            BoundingRectangle.Max.Y = tile.BoundingRectangle.Min.Y;
                            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                            Physics.Velocity = jumpSpeed;
                        }
                    }
                }
                // otherwise, the block is passable, so do not test any collisions for it
                else if (tile.collisionType == Tile.CollisionType.Platform)
                {
                    if (side == 3 && !ridingVehicle)
                    {
                        BoundingRectangle.Min.Y = tile.BoundingRectangle.Min.Y - 80;
                        BoundingRectangle.Max.Y = tile.BoundingRectangle.Min.Y;
                        texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                        Physics.Velocity = jumpSpeed;
                    }
                }
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
            // begin riding vehicle
            if (w.Type == Waypoint.WaypointType.Spinner)
            {
                if (BoundingRectangle.Intersects(w.BoundingRectangle))
                {
                    ridingVehicle = true;
                    BoundingRectangle.Min = new Vector3(w.BoundingRectangle.Min.X, w.BoundingRectangle.Min.Y - 80, 0.0f);
                    BoundingRectangle.Max = new Vector3(w.BoundingRectangle.Max.X, w.BoundingRectangle.Min.Y, 0.0f);
                    texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                }
            }

            else if (w.Type == Waypoint.WaypointType.Lift)
            {
                if (BoundingRectangle.Intersects(w.BoundingRectangle))
                {
                    ridingVehicle = true;
                    BoundingRectangle.Min = new Vector3(w.BoundingRectangle.Min.X, w.BoundingRectangle.Min.Y - 80, 0.0f);
                    BoundingRectangle.Max = new Vector3(w.BoundingRectangle.Max.X, w.BoundingRectangle.Min.Y, 0.0f);
                    texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                }
            }
        }
    }
}
