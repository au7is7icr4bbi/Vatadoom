using System;
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
    /// <summary>
    /// Represents an overworld map that stores 6 levels
    /// </summary>
    class Map : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private List<Level> levels;
        private int currentLevel;
        private Player player;
        private Texture2D mapTexture;
        public Map(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                //player.BoundingRectangle.Offset(new Point(5, 5));
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                //player.BoundingRectangle.Offset(new Point(-5, 5));
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                // start the level
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
