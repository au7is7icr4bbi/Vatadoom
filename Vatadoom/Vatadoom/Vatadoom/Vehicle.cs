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
        public BoundingBox BoundingRectangle;
        public Rectangle texRect;
        private Game g;
        private Texture2D texture;
        private Physics physics;
        private Level level;

        public Vehicle(Game game, Player p, VehicleType type, Vector2 pos, Level curr)
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
            {
                BoundingRectangle = new BoundingBox(new Vector3(pos.X, pos.Y, 0.0f), new Vector3(pos.X + 60, pos.Y + 40, 0.0f));
                texRect = new Rectangle((int)pos.X, (int)pos.Y, 60, 40);
            }
            else
            {
                BoundingRectangle = new BoundingBox(new Vector3(pos.X, pos.Y - 40, 0.0f), new Vector3(pos.X + 300, pos.Y, 0.0f));
                texRect = new Rectangle((int)pos.X, (int)pos.Y - 40, 300, 40);
            }
            level = curr;
        }

        public void resetRectangle(Point spawn)
        {
            BoundingRectangle.Min = new Vector3(spawn.X * 60, spawn.Y * 40, 0.0f);
            BoundingRectangle.Max = new Vector3(spawn.X * 60 + 60, spawn.Y * 40 + 80, 0.0f);
            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
            //player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
            physics.Velocity = 200.0f;
        }

        public void Update(GameTime gameTime)
        {
            if (player.ridingVehicle)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space) && Type != VehicleType.Lift)
                {
                    BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X, BoundingRectangle.Min.Y + -physics.dynamicVerticalMotion(BoundingRectangle.Min, physics.Velocity, gameTime).Y, BoundingRectangle.Min.Z);
                    BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X, BoundingRectangle.Min.Y + 40, BoundingRectangle.Max.Z);
                    player.BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X, BoundingRectangle.Min.Y - 80, BoundingRectangle.Min.Z);
                    player.BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X, BoundingRectangle.Min.Y, BoundingRectangle.Min.Z);
                    texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                    player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                }

                if (Keyboard.GetState().IsKeyUp(Keys.Space) && Type != VehicleType.Lift)
                {
                    if (physics.Velocity > 0.0f)
                    {
                        physics.Velocity = 0.0f;
                    }
                    BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X, BoundingRectangle.Min.Y + -physics.dynamicVerticalMotion(BoundingRectangle.Min, physics.Velocity, gameTime).Y, BoundingRectangle.Min.Z);
                    BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X, BoundingRectangle.Min.Y + 80, BoundingRectangle.Max.Z);
                    player.BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X, BoundingRectangle.Min.Y - 80, BoundingRectangle.Min.Z);
                    player.BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X, BoundingRectangle.Min.Y, BoundingRectangle.Max.Z);
                    texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                    player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                }

                // move right
                if (Keyboard.GetState().IsKeyDown(Keys.D) && Type == VehicleType.Lift)
                {
                    player.BoundingRectangle.Min = new Vector3(player.BoundingRectangle.Min.X + physics.horizontalMotion(player.BoundingRectangle.Min, 200.0f, gameTime).X, player.BoundingRectangle.Min.Y, player.BoundingRectangle.Min.Z);
                    player.BoundingRectangle.Max = new Vector3(player.BoundingRectangle.Min.X + physics.horizontalMotion(player.BoundingRectangle.Max, 200.0f, gameTime).X, player.BoundingRectangle.Max.Y, player.BoundingRectangle.Max.Z);
                    player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                }

                // move left
                if (Keyboard.GetState().IsKeyDown(Keys.A) && Type == VehicleType.Lift)
                {
                    player.BoundingRectangle.Min = new Vector3(player.BoundingRectangle.Min.X + physics.horizontalMotion(player.BoundingRectangle.Min, -200.0f, gameTime).X, player.BoundingRectangle.Min.Y, player.BoundingRectangle.Min.Z);
                    player.BoundingRectangle.Max = new Vector3(player.BoundingRectangle.Min.X + physics.horizontalMotion(player.BoundingRectangle.Max, -200.0f, gameTime).X, player.BoundingRectangle.Max.Y, player.BoundingRectangle.Max.Z);
                    player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                }
                
                // vehicle will always move left to right, so horizontal motion is always positive
                if (Type != Vehicle.VehicleType.Lift)
                {
                    BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X + physics.horizontalMotion(BoundingRectangle.Min, 200.0f, gameTime).X, BoundingRectangle.Min.Y, BoundingRectangle.Min.Z);
                    BoundingRectangle.Max = new Vector3(BoundingRectangle.Min.X + physics.horizontalMotion(BoundingRectangle.Max, 200.0f, gameTime).X, BoundingRectangle.Max.Y, BoundingRectangle.Max.Z);
                    player.BoundingRectangle.Min = new Vector3(player.BoundingRectangle.Min.X, BoundingRectangle.Min.Y - 80, player.BoundingRectangle.Min.Z);
                    player.BoundingRectangle.Max = new Vector3(player.BoundingRectangle.Max.X, BoundingRectangle.Min.Y, player.BoundingRectangle.Max.Z);
                    texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                    player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                }
                else
                {
                    BoundingRectangle.Min = new Vector3(BoundingRectangle.Min.X, BoundingRectangle.Min.Y + -physics.staticVerticalMotion(BoundingRectangle.Min, 100.0f, gameTime).Y, BoundingRectangle.Min.Z);
                    BoundingRectangle.Max = new Vector3(BoundingRectangle.Max.X, BoundingRectangle.Max.Y + -physics.staticVerticalMotion(BoundingRectangle.Max, 100.0f, gameTime).Y, BoundingRectangle.Max.Z);
                    player.BoundingRectangle.Min = new Vector3(player.BoundingRectangle.Min.X, BoundingRectangle.Min.Y - 80, player.BoundingRectangle.Min.Z);
                    player.BoundingRectangle.Max = new Vector3(player.BoundingRectangle.Max.X, BoundingRectangle.Min.Y, player.BoundingRectangle.Max.Z);
                    texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                    player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(texture, texRect, Color.White);
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
                            BoundingRectangle.Min.X = tile.BoundingRectangle.Min.X - 60;
                            BoundingRectangle.Max.X = tile.BoundingRectangle.Min.X;
                            player.BoundingRectangle.Min.X = BoundingRectangle.Min.X;
                            player.BoundingRectangle.Max.X = BoundingRectangle.Max.X;
                            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                            player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                            //BottomBoundingRectangle.Location = new Point(tile.BoundingRectangle.Left - 60, BottomBoundingRectangle.Location.Y);
                        }

                        // colliding with a block to your left
                        if (side == 1)
                        {
                            BoundingRectangle.Min.X = tile.BoundingRectangle.Max.X;
                            BoundingRectangle.Max.X = BoundingRectangle.Min.X + 60;
                            player.BoundingRectangle.Max.X = BoundingRectangle.Max.X;
                            player.BoundingRectangle.Min.X = BoundingRectangle.Min.X;
                            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                            player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                        }

                        if (side == 3)
                        {
                            BoundingRectangle.Min.Y = tile.BoundingRectangle.Min.Y - 40;
                            BoundingRectangle.Max.Y = tile.BoundingRectangle.Min.Y;
                            player.BoundingRectangle.Min.Y = BoundingRectangle.Min.Y - 80;
                            player.BoundingRectangle.Max.Y = BoundingRectangle.Min.Y;
                            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                            player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                            physics.Velocity = 200.0f;
                        }
                    }
                    else if (tile.collisionType == Tile.CollisionType.Platform)
                    {
                        if (side == 3)
                        {
                            BoundingRectangle.Min.Y = tile.BoundingRectangle.Min.Y - 40;
                            BoundingRectangle.Max.Y = tile.BoundingRectangle.Min.Y;
                            player.BoundingRectangle.Min.Y = BoundingRectangle.Min.Y - 80;
                            player.BoundingRectangle.Max.Y = BoundingRectangle.Min.Y;
                            texRect.Location = new Point((int)BoundingRectangle.Min.X, (int)BoundingRectangle.Min.Y);
                            player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
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
                            player.BoundingRectangle.Min = new Vector3(w.TileCoords.X + 60, w.TileCoords.Y - 40, 0.0f);
                            player.BoundingRectangle.Max = new Vector3(player.BoundingRectangle.Min.X + 60, player.BoundingRectangle.Min.Y + 80, 0.0f);
                            player.texRect.Location = new Point((int)player.BoundingRectangle.Min.X, (int)player.BoundingRectangle.Min.Y);
                        }
                    }
                }
            }
        }
    }
}
