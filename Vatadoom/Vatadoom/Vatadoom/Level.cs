using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Level : Microsoft.Xna.Framework.DrawableGameComponent, WaypointHandler
    {
        private Layer[] layers;
        private Tile[][] tiles;
        private List<Texture2D> textures;
        private Player player;
        private int playerLocationX;
        private int playerLocationY;
        private Point spawn; // represents map grid coordinates, for use with save points
        private int levelx = 0;
        private int width;
        private int height;
        private Random r;
        bool levelEnded = false;
        private SpriteBatch spriteBatch;
        private float cameraXPosition;
        private float cameraYPosition;
        public Dictionary<String, Waypoint> waypoints { get; set; }
        public Level(Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            // TODO: Construct any child components here
            textures = new List<Texture2D>();
            levelx = 0;
            this.spriteBatch = spriteBatch;
            r = new Random();
            layers = new Layer[3];
            layers[0] = new Layer(Game.Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(Game.Content, "Backgrounds/Layer1", 0.5f);
            layers[2] = new Layer(Game.Content, "Backgrounds/Layer2", 0.8f);
            waypoints = new Dictionary<String, Waypoint>();
        }

        public void setSpriteBatch(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            base.Initialize();
        }

        protected override void LoadContent()
        {
            textures.Clear();
            // using the level x, load the relevant textures into the texture list
            switch (levelx)
            {
                case 0:
                    // highway
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/road-0"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/road-1"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/concrete"));
                    break;
                case 1:
                    // sewer
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/sewerWall"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/sewerInterior"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/cannon"));
                    break;
                case 2:
                    // tower
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/buildingWall"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/buildingInterior"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/platform"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/cannon"));
                    break;
                case 3:
                    // rooftops
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/buildingWall"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/buildingInterior"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/cannon"));
                    break;
                case 4:
                    // rooftops with spinner
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/buildingWall"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/powerline"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/spinner"));
                    break;
                default:
                    // boss level
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/powerline"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/spinner"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/bandit"));
                    textures.Add(Game.Content.Load<Texture2D>("Tiles/buildingWall"));
                    break;
            }
            StreamReader reader = new StreamReader("Content/Levels/" + levelx + ".txt");
            width = int.Parse(reader.ReadLine());
            height = int.Parse(reader.ReadLine());
            tiles = new Tile[width][];
            for (int i = 0; i < width; i++)
                tiles[i] = new Tile[height];
            loadLevel(reader);
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // if the level has ended, load the next level
            if (levelEnded)
                nextLevel();
            player.Update(gameTime);
            if (player.BoundingRectangle.Bottom >= height * 40)
            {
                // player has fell to their death. Respawn
                player.resetRectangle(spawn);
            }
            if (player.BoundingRectangle.Left <= 0)
            {
                player.BoundingRectangle.Offset(-(player.BoundingRectangle.Left), 0);
            }
            playerLocationX = (int)Math.Round((double)player.BoundingRectangle.Left / 60.0);
            playerLocationY = (int)Math.Round((double)player.BoundingRectangle.Top / 40.0);

            // block immediately to the right, at head height
            if (playerLocationX + 1 < width)
            {
                if (playerLocationY + 1 < height)
                    player.testCollisions(tiles[playerLocationX + 1][playerLocationY + 1], 0, gameTime);
                player.testCollisions(tiles[playerLocationX + 1][playerLocationY], 0, gameTime);
            }

            // block immediately to the left, at head height
            if (playerLocationX - 1 >= 0)
            {
                if (playerLocationY + 1 < height)
                    player.testCollisions(tiles[playerLocationX - 1][playerLocationY - 1], 1, gameTime);
                player.testCollisions(tiles[playerLocationX - 1][playerLocationY], 1, gameTime);
            }

            if (playerLocationY + 2 < height)
            {
                // block below the player
                player.testCollisions(tiles[playerLocationX][playerLocationY + 2], 3, gameTime);
            }

            // block immediately above the player
            if (playerLocationY - 1 >= 0)
            {
                player.testCollisions(tiles[playerLocationX][playerLocationY - 1], 2, gameTime);
            }
            
            // search around the player for collisions with tiles

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            /*
            spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();*/

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraXPosition, -cameraYPosition, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, cameraTransform);
            player.Draw(spriteBatch);
            
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraXPosition / 60.0f);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / 60;
            int top = (int)Math.Floor(cameraYPosition / 40.0f);
            int bottom = top + spriteBatch.GraphicsDevice.Viewport.Height / 40;
            right = Math.Min(right, width - 1);
            bottom = Math.Min(bottom, height - 1);
            
            // draw every tile
            for (int i = left; i <= right; i++)
                for (int j = top; j <= bottom; j++)
                    if (tiles[i][j] != null)
                        tiles[i][j].Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = /*EntityLayer + 1*/0; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraXPosition);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void loadLevel(StreamReader reader)
        {
            int currentTile = 0;
            currentTile = reader.Read();
            bool swap = true;
            int x = 0;
            int y = 0;
            while (!reader.EndOfStream)
            {
                switch (currentTile)
                {
                    case 'w':
                        // waypoint
                        tiles[x][y] = new Tile(Game, new Vector2(x * 60.0f, y * 40), Tile.TileType.Waypoint);
                        break;
                    case 'p':
                        // player tile. Spawn the player here
                        tiles[x][y] = new Tile(Game, new Vector2(x * 60, y * 40), Tile.TileType.Player);
                        player = new Player(Game, new Vector2(x * 60, y * 40), this);
                        playerLocationX = x;
                        playerLocationY = y;
                        spawn = new Point(x, y);
                        break;
                    case 'P':
                        // platform
                        tiles[x][y] = new Tile(Game, new Vector2(x * 60.0f, y * 40), Tile.TileType.Platform);
                        break;
                    case '.':
                        // air
                        tiles[x][y] = new Tile(Game, new Vector2(x * 60.0f, y * 40), Tile.TileType.Air);
                        break;
                    case 'x':
                        // road
                        // swap between the two different textures
                        if (swap)
                            tiles[x][y] = new Tile(Game, textures.ElementAt(0), new Vector2(x * 60.0f, y * 40), Tile.TileType.Road);
                        else
                            tiles[x][y] = new Tile(Game, textures.ElementAt(1), new Vector2(x * 60.0f, y * 40), Tile.TileType.Road);
                        break;
                    case 'c':
                        tiles[x][y] = new Tile(Game, textures.ElementAt(2), new Vector2(x * 60.0f, y * 40), Tile.TileType.Concrete);
                        break;
                    case 's':
                        tiles[x][y] = new Tile(Game, new Vector2(x * 60.0f, y * 40), Tile.TileType.Waypoint);
                        waypoints.Add("s", new Waypoint(Waypoint.WaypointType.SavePoint, this, new Point(x, y)));
                        break;
                    case '\n':
                        // newline or EOF, reset the x value and increment the y value
                        y++;
                        x = -1;
                        break;
                    default: 
                        // carriage return, reset the x-value once more
                        x = -1;
                        break;
                }
                x++;
                swap = !swap;
                currentTile = reader.Read();
            }
        }

        private void nextLevel()
        {
            levelx++;
            LoadContent();
        }
        
        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.35f;

            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraXPosition + marginWidth;
            float marginRight = cameraXPosition + viewport.Width - marginWidth;
            
            const float TopMargin = 0.3f;
            const float BottomMargin = 0.3f;
            float marginTop = cameraYPosition + viewport.Height * TopMargin;
            float marginBottom = cameraYPosition + viewport.Height - viewport.Height * BottomMargin;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraHMovement = 0.0f;
            float cameraVMovement = 0.0f;

            if (player.BoundingRectangle.X < marginLeft)
                cameraHMovement = player.BoundingRectangle.X - marginLeft;
            else if (player.BoundingRectangle.X > marginRight)
                cameraHMovement = player.BoundingRectangle.X - marginRight;
            if (player.BoundingRectangle.Y < marginTop)
                cameraVMovement = player.BoundingRectangle.Y - marginTop;
            else if (player.BoundingRectangle.Y > marginBottom)
                cameraVMovement = player.BoundingRectangle.Y - marginBottom;

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraWidthPosition = 60.0f * width - viewport.Width;
            float maxCameraHeightPosition = 40.0f * height - viewport.Height;
            cameraXPosition = MathHelper.Clamp(cameraXPosition + cameraHMovement, 0.0f, maxCameraWidthPosition);
            cameraYPosition = MathHelper.Clamp(cameraYPosition + cameraVMovement, 0.0f, maxCameraHeightPosition);
        }
        
        public void handleEvent(Waypoint w)
        {
            if (w.Type == Waypoint.WaypointType.SavePoint)
            {
                Point oldSpawn = spawn;
                Vector2 pos = new Vector2(((float)oldSpawn.X * 60.0f), (float)(oldSpawn.Y * 40.0f));
                tiles[oldSpawn.X][oldSpawn.Y] = new Tile(this.Game, pos, Tile.TileType.Air);
                spawn = w.TileCoords;
                tiles[spawn.X][spawn.Y] = new Tile(this.Game, new Vector2((float)spawn.X * 60.0f, (float)spawn.Y * 40.0f), Tile.TileType.Player);
            }
        }
    }
}
