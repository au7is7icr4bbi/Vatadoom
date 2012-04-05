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
    /// Manages physics calculations to the object it is attached to
    /// </summary>
    public class Physics
    {
        private const float gravity = -40.0f;
        public float Velocity { get; set; }
        
        /// <summary>
        /// Initialise the physics object by setting the chain to 0.0f
        /// </summary>
        public Physics()
        {
            // TODO: Construct any child components here
            Velocity = 0.0f;
        }

        /// <summary>
        /// Calculate vertical motion influenced by gravity
        /// </summary>
        /// <param name="startPos">Starting position</param>
        /// <param name="velocity">Velocity (this can be chained for gravity and jump arcs using the built-in Velocity property)</param>
        /// <param name="gameTime">Current game time</param>
        /// <returns>The delta between the starting point and ending point</returns>
        public Vector2 dynamicVerticalMotion(Point startPos, float velocity, GameTime gameTime)
        {
            Vector2 endPos = new Vector2();
            endPos.X = startPos.X;
            endPos.Y = (velocity * (float)gameTime.ElapsedGameTime.TotalSeconds + 0.5f * gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * (float)gameTime.ElapsedGameTime.TotalSeconds));
            Velocity = velocity + gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            return endPos;
        }

        /// <summary>
        /// Calculate the constant horizontal motion
        /// </summary>
        /// <param name="startPos">Starting position</param>
        /// <param name="velocity">Velocity (does not need chaining)</param>
        /// <param name="gameTime">Current game time</param>
        /// <returns>The delta between the starting point and the ending point</returns>
        public Vector2 horizontalMotion(Point startPos, float velocity, GameTime gameTime)
        {
            Vector2 endPos = new Vector2();
            endPos.X = velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            endPos.Y = startPos.Y;
            return endPos;
        }

        public Vector2 staticVerticalMotion(Point startPos, float velocity, GameTime gameTime)
        {
            Vector2 endPos = new Vector2();
            endPos.X = startPos.X;
            endPos.Y = velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            return endPos;
        }
    }
}
