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
    class Player
    {
        public Rectangle BoundingRectangle;
        private Texture2D texture;
        private Physics physics;
        private Level currentLevel;
        private float jumpSpeed = 200.0f;
        private float moveSpeed = 200.0f;
        bool canClimb = false;
        bool climbing = false;
        public Player(Game game, Vector2 pos, Level level)
        {
            texture = game.Content.Load<Texture2D>("Player/player");
            BoundingRectangle = new Rectangle((int)pos.X, (int)pos.Y, 60, 80);
            physics = new Physics();
            physics.Velocity = jumpSpeed;
            currentLevel = level;
        }

        /// <summary>
        /// Resets the player location to the last saved spawn point
        /// </summary>
        /// <param name="spawn">Point specifying spawn position in level grid</param>
        public void resetRectangle(Point spawn)
        {
            BoundingRectangle.Location = new Point(spawn.X * 60, spawn.Y * 40);
            physics.Velocity = jumpSpeed;
        }

        public void Update(GameTime gameTime)
        {
            // move right
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                BoundingRectangle.Offset((int)physics.horizontalMotion(BoundingRectangle.Center, moveSpeed, gameTime).X, 0);
            }

            // move left
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                BoundingRectangle.Offset((int)physics.horizontalMotion(BoundingRectangle.Center, -moveSpeed, gameTime).X, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                if (canClimb)
                {
                    climbing = true;
                    BoundingRectangle.Offset(0, -(int)physics.staticVerticalMotion(BoundingRectangle.Center, moveSpeed, gameTime).Y);
                }
            }

            if (Keyboard.GetState().IsKeyUp(Keys.W))
            {
                climbing = false;
            }

            // jump under the effects of gravity
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(BoundingRectangle.Center, physics.Velocity, gameTime).Y));
            }

            // simulate gravity
            if (Keyboard.GetState().IsKeyUp(Keys.Space))
            {
                if (!climbing)
                {
                    if (physics.Velocity > 0.0f)
                        physics.Velocity = 0.0f;
                    BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(BoundingRectangle.Center, physics.Velocity, gameTime).Y));
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

                    else
                    {
                        if (canClimb)
                            canClimb = !canClimb;
                        // block movement through the block
                        // colliding with a block to your right
                        if (BoundingRectangle.Right > tile.BoundingRectangle.Left && side == 0)
                            BoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, BoundingRectangle.Location.Y);

                        // colliding with a block to your left
                        else if (BoundingRectangle.Left < tile.BoundingRectangle.Right && side == 1)
                            BoundingRectangle.Location = new Point(tile.BoundingRectangle.Right, BoundingRectangle.Location.Y);

                        // hitting a solid block from beneath, so block movement
                        else if (BoundingRectangle.Top < tile.BoundingRectangle.Bottom && side == 2)
                        {
                            BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, tile.BoundingRectangle.Bottom);
                            if (physics.Velocity > 0.0f)
                                physics.Velocity = 0.0f;
                        }

                        // akin to landing on top of a solid block or platform
                        else if (BoundingRectangle.Bottom > tile.BoundingRectangle.Top && side == 3)
                        {
                            BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, tile.BoundingRectangle.Top - 80);
                            physics.Velocity = jumpSpeed;
                        }
                    }
                }
                else if (tile.collisionType == Tile.CollisionType.Platform)
                {
                    // akin to landing on top of a platform block. No other collisions need to be detected
                    if (BoundingRectangle.Bottom > tile.BoundingRectangle.Top && side == 3)
                    {
                        BoundingRectangle.Offset(0, -(BoundingRectangle.Bottom - tile.BoundingRectangle.Top));
                        physics.Velocity = jumpSpeed;
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
    }
}
