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
    class Vehicle : WaypointHandler
    {
        public enum VehicleType
        {
            Spinner,
            Jeep,
            Minecart,
            Rocket,
            Lift
        };
        public VehicleType Type { get; private set; }
        private Player player;
        public Rectangle BoundingRectangle;
        private Game g;
        private Texture2D texture;
        private Physics physics;
        private Level level;

        public Vehicle(Game game, ref Player p, VehicleType type, Vector2 pos, Level curr)
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
                case VehicleType.Lift:
                    texture = g.Content.Load<Texture2D>("Vehicles/lift");
                    break;
            }
            physics = new Physics();
            physics.Velocity = 200.0f;
            // create the bounding rectangle in world space
            if (Type != VehicleType.Lift)
                BoundingRectangle = new Rectangle((int)pos.X, (int)pos.Y, 60, 40);
            else
                BoundingRectangle = new Rectangle((int)pos.X, (int)pos.Y - 40, 300, 40);
            level = curr;
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
                if (Keyboard.GetState().IsKeyDown(Keys.Space) && Type != VehicleType.Lift)
                {
                    BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(BoundingRectangle.Center, physics.Velocity, gameTime).Y));
                    player.BoundingRectangle.Location = new Point(BoundingRectangle.Left, BoundingRectangle.Bottom - 80);
                }

                if (Keyboard.GetState().IsKeyUp(Keys.Space) && Type != VehicleType.Lift)
                {
                    if (physics.Velocity > 0.0f)
                    {
                        physics.Velocity = 0.0f;
                    }
                    BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(BoundingRectangle.Center, physics.Velocity, gameTime).Y));
                    player.BoundingRectangle.Location = new Point(BoundingRectangle.Left, BoundingRectangle.Top - 40);
                }

                // move right
                if (Keyboard.GetState().IsKeyDown(Keys.D) && Type == VehicleType.Lift)
                {
                    player.BoundingRectangle.Offset((int)physics.horizontalMotion(player.BoundingRectangle.Center, 200.0f, gameTime).X, 0);
                }

                // move left
                if (Keyboard.GetState().IsKeyDown(Keys.A) && Type == VehicleType.Lift)
                {
                    player.BoundingRectangle.Offset((int)physics.horizontalMotion(player.BoundingRectangle.Center, -200.0f, gameTime).X, 0);
                }
                
                // vehicle will always move left to right, so horizontal motion is always positive
                if (Type != Vehicle.VehicleType.Lift)
                {
                    BoundingRectangle.Offset((int)physics.horizontalMotion(BoundingRectangle.Center, 200.0f, gameTime).X, 0);
                    player.BoundingRectangle.Location = new Point(BoundingRectangle.Left, player.BoundingRectangle.Top - 40);
                }
                else
                {
                    BoundingRectangle.Offset(0, -(int)physics.staticVerticalMotion(BoundingRectangle.Center, 100.0f, gameTime).Y);
                    player.BoundingRectangle.Location = new Point(player.BoundingRectangle.Left, BoundingRectangle.Top - 80);
                }
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
                    if (tile.tileType == Tile.TileType.Waypoint)
                    {
                        for(int i = 0; i < level.waypoints.Values.Count; i++)
                        {
                            level.waypoints.ElementAt(i).Value.handleEvent();
                        }
                    }
                    if (tile.collisionType == Tile.CollisionType.Solid)
                    {
                        // if (tile.tileType == Tile.TileType.Bullet)
                        // damage the player
                        // block movement through the block
                        // colliding with a block to your right
                        if (side == 0)
                        {
                            BoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, BoundingRectangle.Location.Y);
                            player.BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, BoundingRectangle.Top - 80);
                            //BottomBoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, BottomBoundingRectangle.Location.Y);
                        }

                        // colliding with a block to your left
                        if (side == 1)
                        {
                            BoundingRectangle.Location = new Point(tile.BoundingRectangle.Right, BoundingRectangle.Location.Y);
                            player.BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, BoundingRectangle.Top - 80);
                        }

                        if (side == 3)
                        {
                            BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, tile.BoundingRectangle.Top - 40);
                            player.BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, BoundingRectangle.Top - 80);
                            physics.Velocity = 200.0f;
                        }
                    }
                    else if (tile.collisionType == Tile.CollisionType.Platform)
                    {
                        if (side == 3)
                        {
                            BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, tile.BoundingRectangle.Top - 40);
                            player.BoundingRectangle.Location = new Point(BoundingRectangle.Location.X, BoundingRectangle.Top - 80);
                            physics.Velocity = 200.0f;
                        }
                    }
                }
            }
        }
        public void handleEvent(Waypoint w)
        {
            if (w.Type == Waypoint.WaypointType.EndRide)
            {
                if (BoundingRectangle.Intersects(w.BoundingRectangle))
                {
                    if (player.ridingVehicle)
                    {
                        if (this.Type == VehicleType.Lift)
                        {
                            player.ridingVehicle = false;
                        }
                        else
                        {
                            player.ridingVehicle = false;
                            player.BoundingRectangle.Location = new Point(w.TileCoords.X * 60 + 1, w.TileCoords.Y * 40 - 40);
                        }
                    }
                }
            }
        }
    }
}
