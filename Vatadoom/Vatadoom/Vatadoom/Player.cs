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
        private Rectangle rectangle;
        public Rectangle BoundingRectangle { get { return rectangle; } }
        private Texture2D texture;
        private Physics physics;
        private Level currentLevel;
        private float jumpSpeed = 200.0f;
        private float moveSpeed = 200.0f;
        public Player(Game game, Vector2 pos, Level level)
        {
            texture = game.Content.Load<Texture2D>("Player/player");
            rectangle = new Rectangle((int)pos.X, (int)pos.Y, texture.Width, texture.Height);
            physics = new Physics();
            physics.Velocity = jumpSpeed;
            currentLevel = level;
        }

        // resets the player to the spawn point
        public void resetRectangle(Point spawn)
        {
            rectangle.X = spawn.X * 60;
            rectangle.Y = spawn.Y * 40;
            rectangle.Width = 60;
            rectangle.Height = 80;
            physics.Velocity = jumpSpeed;
        }

        public void Update(GameTime gameTime)
        {
            // move right
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                rectangle.Offset((int)physics.horizontalMotion(rectangle.Center, moveSpeed, gameTime).X, 0);
            }

            // move left
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                rectangle.Offset((int)physics.horizontalMotion(rectangle.Center, -moveSpeed, gameTime).X, 0);
            }

            // jump under the effects of gravity
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                 rectangle.Offset(0, -((int)physics.dynamicVerticalMotion(rectangle.Center, physics.Velocity, gameTime).Y));
            }

            // simulate gravity
            if (Keyboard.GetState().IsKeyUp(Keys.Space))
            {
                if (physics.Velocity > 0.0f)
                    physics.Velocity = 0.0f;
                rectangle.Offset(0, -((int)physics.dynamicVerticalMotion(rectangle.Center, physics.Velocity, gameTime).Y));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, BoundingRectangle, Color.White);
        }

        public void testCollisions(Tile tile, int side, GameTime gameTime)
        {
            if (BoundingRectangle.Intersects(tile.BoundingRectangle))
            {
                // collision detected, process it
                if (tile.collisionType == Tile.CollisionType.Solid)
                {
                    if (tile.tileType == Tile.TileType.Waypoint)
                    {
                        Waypoint waypoint = null;
                        currentLevel.waypoints.TryGetValue("s", out waypoint);
                        waypoint.handleEvent();
                    }

                    else
                    {
                        // block movement through the block
                        // colliding with a block to your right
                        if (BoundingRectangle.Right > tile.BoundingRectangle.Left && side == 0)
                            rectangle.Offset(-(BoundingRectangle.Right - tile.BoundingRectangle.Left), 0);

                        // colliding with a block to your left
                        else if (BoundingRectangle.Left < tile.BoundingRectangle.Right && side == 1)
                            rectangle.Offset(-(BoundingRectangle.Left - tile.BoundingRectangle.Right), 0);

                        // hitting a solid block from beneath, so block movement
                        else if (BoundingRectangle.Top < tile.BoundingRectangle.Bottom && side == 2)
                            rectangle.Offset(0, -(BoundingRectangle.Top - tile.BoundingRectangle.Bottom));

                        // akin to landing on top of a solid block or platform
                        else if (BoundingRectangle.Bottom > tile.BoundingRectangle.Top && side == 3)
                        {
                            rectangle.Offset(0, -(BoundingRectangle.Bottom - tile.BoundingRectangle.Top));
                            physics.Velocity = jumpSpeed;
                        }
                    }
                }
                else if (tile.collisionType == Tile.CollisionType.Platform)
                {
                    // akin to landing on top of a platform block. No other collisions need to be detected
                    if (BoundingRectangle.Bottom > tile.BoundingRectangle.Top && side == 3)
                    {
                        rectangle.Offset(0, -(BoundingRectangle.Bottom - tile.BoundingRectangle.Top));
                        physics.Velocity = jumpSpeed;
                    }
                }
                // otherwise, the block is passable, so do not test any collisions for it
            }
        }
    }
}
