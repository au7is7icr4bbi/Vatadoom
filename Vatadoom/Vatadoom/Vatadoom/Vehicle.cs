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
        public Rectangle BoundingRectangle { get; set; }
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
            // create the bounding rectangle in world space
            BoundingRectangle = new Rectangle((int)pos.X, (int)pos.Y, texture.Width, texture.Height);
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                player.BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(player.BoundingRectangle.Center, player.Physics.Velocity, gameTime).Y));
                BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(BoundingRectangle.Center, physics.Velocity, gameTime).Y));
            }

            if (Keyboard.GetState().IsKeyUp(Keys.Space))
            {
                if (physics.Velocity > 0.0f)
                {
                    physics.Velocity = 0.0f;
                    player.Physics.Velocity = 0.0f;
                }
                player.BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(player.BoundingRectangle.Center, player.Physics.Velocity, gameTime).Y));
                BoundingRectangle.Offset(0, -((int)physics.dynamicVerticalMotion(BoundingRectangle.Center, physics.Velocity, gameTime).Y));
            }
            // vehicle will always move left to right, so horizontal motion is always positive
            player.BoundingRectangle.Offset((int)physics.horizontalMotion(player.BoundingRectangle.Center, 5.0f, gameTime).X, 0);
            BoundingRectangle.Offset((int)physics.horizontalMotion(BoundingRectangle.Center, 5.0f, gameTime).X, 0);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(texture, BoundingRectangle, Color.White);
        }
    }
}
