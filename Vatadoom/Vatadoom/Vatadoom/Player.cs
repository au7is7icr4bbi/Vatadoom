﻿using System;
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
        public Rectangle BoundingRectangle;
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
            BoundingRectangle = new Rectangle((int)pos.X, (int)pos.Y, texture.Width, texture.Height);
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
            BoundingRectangle.Location = new Point(spawn.X * 60, spawn.Y * 40);
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
                    BoundingRectangle.Offset((int)Physics.horizontalMotion(BoundingRectangle.Center, moveSpeed, gameTime).X, 0);
                }

                // move left
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    BoundingRectangle.Offset((int)Physics.horizontalMotion(BoundingRectangle.Center, -moveSpeed, gameTime).X, 0);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    if (canClimb)
                    {
                        climbing = true;
                        BoundingRectangle.Offset(0, -(int)Physics.staticVerticalMotion(BoundingRectangle.Center, moveSpeed, gameTime).Y);
                    }
                }

                if (Keyboard.GetState().IsKeyUp(Keys.W))
                {
                    climbing = false;
                }

                // jump under the effects of gravity
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    BoundingRectangle.Offset(0, -((int)Physics.dynamicVerticalMotion(BoundingRectangle.Center, Physics.Velocity, gameTime).Y));
                }

                // simulate gravity
                if (Keyboard.GetState().IsKeyUp(Keys.Space))
                {
                    if (!climbing)
                    {
                        if (Physics.Velocity > 0.0f)
                            Physics.Velocity = 0.0f;
                        BoundingRectangle.Offset(0, -((int)Physics.dynamicVerticalMotion(BoundingRectangle.Center, Physics.Velocity, gameTime).Y));
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, BoundingRectangle, Color.White);
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
                            BoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, BoundingRectangle.Location.Y);
                        }

                        // colliding with a block to your left
                        else if (side == 1)
                        {
                            BoundingRectangle.Location = new Point(tile.BoundingRectangle.Right, BoundingRectangle.Location.Y);
                        }

                        // hitting a solid block from beneath, so block movement
                        else if (side == 2)
                        {
                            BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, tile.BoundingRectangle.Bottom);
                            if (Physics.Velocity > 0.0f)
                                Physics.Velocity = 0.0f;
                        }

                        else if (side == 3 && !ridingVehicle)
                        {
                            BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, tile.BoundingRectangle.Top - 80);
                            Physics.Velocity = jumpSpeed;
                        }
                    }
                }
                // otherwise, the block is passable, so do not test any collisions for it
                else if (tile.collisionType == Tile.CollisionType.Platform)
                {
                    if (side == 3 && !ridingVehicle)
                    {
                        BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, tile.BoundingRectangle.Top - 80);
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
                    BoundingRectangle.Location = new Point(w.BoundingRectangle.X, w.BoundingRectangle.Y - 80);
                }
            }

            else if (w.Type == Waypoint.WaypointType.Lift)
            {
                if (BoundingRectangle.Intersects(w.BoundingRectangle))
                {
                    ridingVehicle = true;
                    BoundingRectangle.Location = new Point(BoundingRectangle.X, w.BoundingRectangle.Y - 80);
                }
            }
        }
    }
}
