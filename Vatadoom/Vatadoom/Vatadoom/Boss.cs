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
    public class Boss
    {
        private Game game;
        private int health;
        private Level owner;
        private Texture2D texture;
        public BoundingBox BoundingRectangle { get; private set; }
        private Rectangle texRect;
        public Boss(Game game, int x, int y, Level currentLevel)
        {
            health = 100;
            this.game = game;
            texture = game.Content.Load<Texture2D>("Bosses/1");
            BoundingRectangle = new BoundingBox(new Vector3(x, y, 0.0f), new Vector3(x + texture.Width, y + texture.Height, 0.0f));
            texRect = new Rectangle(x, y, texture.Width, texture.Height);
            owner = currentLevel;
        }
        public void onDefeated()
        {
            MediaPlayer.Stop();
            Song song = game.Content.Load<Song>("Music/BossDefeated");
            MediaPlayer.IsRepeating = false;
            MediaPlayer.Play(song);
            while (MediaPlayer.State == MediaState.Playing)
                ;
            owner.nextLevel();
        }

        public void Initialize()
        {
        }

        public void Update(GameTime gameTime)
        {
            // if the tile to its bottom left is passable (ie. air or a background block), move the other way
            // if the tile to its bottom right is passable (ie. air or a background tile, move the other way
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(texture, new Vector2(BoundingRectangle.Min.X, BoundingRectangle.Min.Y), Color.White);
        }
    }
}
