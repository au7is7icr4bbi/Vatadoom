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
    class Vehicle
    {
        public enum VehicleType
        {
            Spinner,
            Jeep,
            Minecart,
            Rocket
        };
        public VehicleType Type { get; private set; }
        private Player player;
        public Rectangle BoundingRectangle;
        private Game g;
        private Texture2D texture;
        private Physics physics;

        public Vehicle(Game game, Player p, VehicleType type, Vector2 pos)
        {
            g = game;
            player = p;
            Type = type;
            switch (Type)
            {
                case VehicleType.Spinner:
                    texture = g.Content.Load<Texture2D>("Vehicles/spinner");
                    break;
                case VehicleType.Jeep:
                    texture = g.Content.Load<Texture2D>("Vehicles/jeep");
                    break;
                case VehicleType.Minecart:
                    texture = g.Content.Load<Texture2D>("Vehicles/minecart");
                    break;
                case VehicleType.Rocket:
                    texture = g.Content.Load<Texture2D>("Vehicles/rocket");
                    break;
            }
            physics = new Physics();
            physics.Velocity = 200.0f;
            // create the bounding rectangle in world space
            BoundingRectangle = new Rectangle((int)pos.X, (int)pos.Y, 60, 40);
        }

        public void resetRectangle(Point spawn)
        {
            BoundingRectangle.Location = new Point(spawn.X * 60, spawn.Y * 40);
            physics.Velocity = 200.0f;
        }

        public void Update(GameTime gameTime)
        {
            if (player.ridingVehicle)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(BoundingRectangle.Center, physics.Velocity, gameTime).Y));
                    player.BottomBoundingRectangle.Location = new Point(BoundingRectangle.Left, BoundingRectangle.Top - 40);
                    player.TopBoundingRectangle.Location = new Point(BoundingRectangle.Left, player.BottomBoundingRectangle.Top - 40);
                }

                if (Keyboard.GetState().IsKeyUp(Keys.Space))
                {
                    if (physics.Velocity > 0.0f)
                    {
                        physics.Velocity = 0.0f;
                    }
                    BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(BoundingRectangle.Center, physics.Velocity, gameTime).Y));
                    player.BottomBoundingRectangle.Location = new Point(BoundingRectangle.Left, BoundingRectangle.Top - 40);
                    player.TopBoundingRectangle.Location = new Point(BoundingRectangle.Left, player.BottomBoundingRectangle.Top - 40);
                }
                
                // vehicle will always move left to right, so horizontal motion is always positive
                BoundingRectangle.Offset((int)physics.horizontalMotion(BoundingRectangle.Center, 200.0f, gameTime).X, 0);
                player.BottomBoundingRectangle.Location = new Point(BoundingRectangle.Left, BoundingRectangle.Top - 40);
                player.TopBoundingRectangle.Location = new Point(BoundingRectangle.Left, player.BottomBoundingRectangle.Top - 40);
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(texture, BoundingRectangle, Color.White);
        }

        public void testCollisions(Tile tile, int side, GameTime gameTime)
        {
            if (player.ridingVehicle)
            {
                if (BoundingRectangle.Intersects(tile.BoundingRectangle))
                {
                    // collision detected, process it
                    if (tile.collisionType == Tile.CollisionType.Solid)
                    {
                        // if (tile.tileType == Tile.TileType.Bullet)
                        // damage the player
                        // block movement through the block
                        // colliding with a block to your right
                        if (side == 0)
                        {
                            BoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, BoundingRectangle.Location.Y);
                            player.BottomBoundingRectangle.Location = new Point(BoundingRectangle.Location.X, BoundingRectangle.Top - 40);
                            player.TopBoundingRectangle.Location = new Point(BoundingRectangle.Location.X, player.BottomBoundingRectangle.Top - 40);
                            //BottomBoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, BottomBoundingRectangle.Location.Y);
                        }

                        // colliding with a block to your left
                        if (side == 1)
                        {
                            BoundingRectangle.Location = new Point(tile.BoundingRectangle.Right, BoundingRectangle.Location.Y);
                            player.BottomBoundingRectangle.Location = new Point(BoundingRectangle.Location.X, BoundingRectangle.Top - 40);
                            player.TopBoundingRectangle.Location = new Point(BoundingRectangle.Location.X, player.BottomBoundingRectangle.Top - 40);
                            //BottomBoundingRectangle.Location = new Point(tile.BoundingRectangle.Right, BottomBoundingRectangle.Location.Y);
                        }

                        if (side == 3)
                        {
                            BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, tile.BoundingRectangle.Top - 40);
                            player.BottomBoundingRectangle.Location = new Point(BoundingRectangle.Location.X, BoundingRectangle.Top - 40);
                            player.TopBoundingRectangle.Location = new Point(BoundingRectangle.Location.X, player.BottomBoundingRectangle.Top - 40);
                            physics.Velocity = 200.0f;
                        }
                    }
                    else if (tile.collisionType == Tile.CollisionType.Platform)
                    {
                        if (side == 3)
                        {
                            BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, tile.BoundingRectangle.Top - 40);
                            player.BottomBoundingRectangle.Location = new Point(BoundingRectangle.Location.X, BoundingRectangle.Top - 40);
                            player.TopBoundingRectangle.Location = new Point(BoundingRectangle.Location.X, player.BottomBoundingRectangle.Top - 40);
                            physics.Velocity = 200.0f;
                        }
                    }
                }
            }
        }
    }
}
