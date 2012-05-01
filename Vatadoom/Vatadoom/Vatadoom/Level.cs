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
        // private TileLayer[] tileLayers;
        public Boss boss { get; set; }
        private Dictionary<String, Texture2D> textures;
        public Player player { get; private set; }
        public Point spawn { get; set; } // represents map grid coordinates, for use with save points
        private int levelx;
        private int width;
        private int height;
        private Vehicle vehicle;
        private SpriteBatch spriteBatch;
        private float cameraXPosition;
        private float cameraYPosition;
        private float cameraZPosition;
        private Point vehicleSpawn;
        public Dictionary<String, Waypoint> waypoints { get; set; }
        private Song backingTrack;
        public Level(Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            // TODO: Construct any child components here
            textures = new Dictionary<String, Texture2D>();
            levelx = 4;
            this.spriteBatch = spriteBatch;
            layers = new Layer[3];
            layers[0] = new Layer(Game.Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(Game.Content, "Backgrounds/Layer1", 0.5f);
            layers[2] = new Layer(Game.Content, "Backgrounds/Layer2", 0.8f);
            waypoints = new Dictionary<String, Waypoint>();
            vehicle = null;
            boss = null;
            // tileLayers = new TileLayer[3];
            // tileLayers.load(levelx);
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
            waypoints.Clear();
            // using the level x, load the relevant textures into the texture list
            switch (levelx)
            {
                case 0:
                    // highway
                    textures.Add("road0", Game.Content.Load<Texture2D>("Tiles/road-0"));
                    textures.Add("road1", Game.Content.Load<Texture2D>("Tiles/road-1"));
                    textures.Add("concrete", Game.Content.Load<Texture2D>("Tiles/concrete"));
                    break;
                case 1:
                    // sewer
                    textures.Add("sewerWall", Game.Content.Load<Texture2D>("Tiles/sewerWall"));
                    textures.Add("sewerInterior", Game.Content.Load<Texture2D>("Tiles/sewerInterior"));
                    //textures.Add("water", Game.Content.Load<Texture2D>("Tiles/water"));
                    //textures.Add("cannon", Game.Content.Load<Texture2D>("Tiles/cannon")); // the cannon is a waypoint block with a texture
                    textures.Add("concrete",Game.Content.Load<Texture2D>("Tiles/concrete"));
                    break;
                case 2:
                    // tower
                    textures.Add("sewerWall", Game.Content.Load<Texture2D>("Tiles/sewerWall"));
                    textures.Add("sewerInterior", Game.Content.Load<Texture2D>("Tiles/sewerInterior"));
                    textures.Add("buildingWall", Game.Content.Load<Texture2D>("Tiles/buildingWall"));
                    textures.Add("buildingInterior", Game.Content.Load<Texture2D>("Tiles/buildingInterior"));
                    textures.Add("concrete", Game.Content.Load<Texture2D>("Tiles/concrete"));
                    textures.Add("buildingPlatform", Game.Content.Load<Texture2D>("Tiles/buildingPlatform"));
                    textures.Add("ladder", Game.Content.Load<Texture2D>("Tiles/ladder"));
                    //textures.Add("cannon", Game.Content.Load<Texture2D>("Tiles/cannon"));
                    break;
                case 3:
                    // rooftops
                    textures.Add("buildingWall", Game.Content.Load<Texture2D>("Tiles/buildingWall"));
                    textures.Add("buildingInterior", Game.Content.Load<Texture2D>("Tiles/buildingInterior"));
                    textures.Add("buildingPlatform", Game.Content.Load<Texture2D>("Tiles/buildingPlatform"));
                    textures.Add("ladder", Game.Content.Load<Texture2D>("Tiles/ladder"));
                    //textures.Add("cannon", Game.Content.Load<Texture2D>("Tiles/cannon"));
                    break;
                case 4:
                    // rooftops with spinner
                    textures.Add("buildingWall", Game.Content.Load<Texture2D>("Tiles/buildingWall"));
                    textures.Add("buildingInterior", Game.Content.Load<Texture2D>("Tiles/buildingInterior"));
                    textures.Add("powerline", Game.Content.Load<Texture2D>("Tiles/powerline"));
                    break;
                case 5: // case 5
                    // boss level
                    textures.Add("powerline",Game.Content.Load<Texture2D>("Tiles/powerline"));
                    textures.Add("buildingWall", Game.Content.Load<Texture2D>("Tiles/buildingWall"));
                    textures.Add("buildingInterior", Game.Content.Load<Texture2D>("Tiles/buildingInterior"));
                    textures.Add("buildingPlatform", Game.Content.Load<Texture2D>("Tiles/buildingPlatform"));
                    break;
                default:
                    // end the game
                    Game.Exit();
                    break;
            }
            if (levelx < 6)
            {
                backingTrack = Game.Content.Load<Song>("Music/" + levelx);
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(backingTrack);
                StreamReader reader = new StreamReader("Content/Levels/" + levelx + ".txt");
                width = int.Parse(reader.ReadLine());
                height = int.Parse(reader.ReadLine());
                tiles = new Tile[width][];
                for (int i = 0; i < width; i++)
                    tiles[i] = new Tile[height];
                loadLevel(reader);
                base.LoadContent();
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            player.Update(gameTime);

            if (player.BoundingRectangle.Max.Y >= height * 40)
            {
                // player has fell to their death. Respawn
                player.resetRectangle(spawn);
            }

            else
            {
                // block below the player
                player.testCollisions(tiles[(int)player.BoundingRectangle.Min.X / 60 - 1][(int)player.BoundingRectangle.Max.Y / 40], 3, gameTime);
                player.testCollisions(tiles[((int)player.BoundingRectangle.Min.X) / 60][(int)player.BoundingRectangle.Max.Y / 40], 3, gameTime);
                player.testCollisions(tiles[(int)player.BoundingRectangle.Min.X / 60 + 1][(int)player.BoundingRectangle.Max.Y / 40], 3, gameTime);
            }

            if (player.BoundingRectangle.Min.Y <= 0)
            {
                // block vertical movement off the game screen
                player.BoundingRectangle.Min.Y = 0;
            }
            else
            {
                // block immediately above the player
                //player.testCollisions(tiles[player.BoundingRectangle.Left / 60][player.BoundingRectangle.Top / 40], 2, gameTime);
                player.testCollisions(tiles[(int)player.BoundingRectangle.Min.X / 60][(int)player.BoundingRectangle.Min.Y / 40 - 1], 2, gameTime);
                //player.testCollisions(tiles[player.BoundingRectangle.Right / 60][player.BoundingRectangle.Top / 40 + 1], 2, gameTime);
            }

            // block movement off the screen
            if (player.BoundingRectangle.Min.X <= 0)
            {
                player.BoundingRectangle.Min.X = 0;
            }

            else
            {
                // block immediately to the left
                player.testCollisions(tiles[(int)player.BoundingRectangle.Min.X / 60][(int)player.BoundingRectangle.Min.Y / 40], 1, gameTime); // leg height
                player.testCollisions(tiles[(int)player.BoundingRectangle.Min.X / 60][(int)player.BoundingRectangle.Min.Y / 40 + 1], 1, gameTime); // head height
            }

            if (player.BoundingRectangle.Max.X >= width * 60)
            {
                player.BoundingRectangle.Max.X = width * 60 - 60;
            }

            else
            {
                // block immediately to the right, at head height
                player.testCollisions(tiles[(int)player.BoundingRectangle.Max.X / 60][(int)player.BoundingRectangle.Min.Y / 40], 0, gameTime); // leg height
                player.testCollisions(tiles[(int)player.BoundingRectangle.Max.X / 60][(int)player.BoundingRectangle.Min.Y / 40 + 1], 0, gameTime); // head height
            }

            // detect internal collisions. Used for waypoints
            player.testCollisions(tiles[((int)player.BoundingRectangle.Min.X + 30) / 60][((int)player.BoundingRectangle.Min.Y + 40) / 40 - 1], 4, gameTime);
            player.testCollisions(tiles[((int)player.BoundingRectangle.Min.X + 30) / 60][((int)player.BoundingRectangle.Min.Y + 40) / 40], 4, gameTime);

            if (vehicle != null && player.ridingVehicle)
            {
                vehicle.Update(gameTime);
                if (vehicle.Type != Vehicle.VehicleType.Lift)
                {
                    vehicle.testCollisions(tiles[(int)vehicle.BoundingRectangle.Min.X / 60 - 1][(int)vehicle.BoundingRectangle.Min.Y / 40], 1, gameTime);
                    vehicle.testCollisions(tiles[(int)vehicle.BoundingRectangle.Max.X / 60 + 1][(int)vehicle.BoundingRectangle.Min.Y / 40], 0, gameTime);
                }
                else
                {
                    vehicle.testCollisions(tiles[(int)vehicle.BoundingRectangle.Min.X / 60][(int)vehicle.BoundingRectangle.Min.Y / 40], 4, gameTime);
                }
                if (vehicle.BoundingRectangle.Max.Y >= height * 40)
                {
                    player.resetRectangle(spawn);
                    vehicle.resetRectangle(vehicleSpawn);
                }
                else
                {
                    if (vehicle.Type != Vehicle.VehicleType.Lift)
                        vehicle.testCollisions(tiles[(int)vehicle.BoundingRectangle.Min.X / 60][(int)vehicle.BoundingRectangle.Max.Y / 40 - 1], 3, gameTime);
                }
            }

            if (boss != null && player.BoundingRectangle.Intersects(boss.BoundingRectangle))
            {
                boss.onDefeated();
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            for (int i = 0; i <= 2; ++i)
                layers[i].Draw(spriteBatch, cameraXPosition);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraXPosition, -cameraYPosition, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, cameraTransform);
            
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

            player.Draw(spriteBatch);
            if (boss != null)
                boss.Draw(spriteBatch, gameTime);
            if (vehicle != null)
                vehicle.Draw(spriteBatch, gameTime);
            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = 3; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraXPosition);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Load the current level from the text file
        /// </summary>
        /// <param name="reader">The level file stream (after width and height have been read in)</param>
        private void loadLevel(StreamReader reader)
        {
            int currentTile = 0;
            currentTile = reader.Read();
            bool swap = true;
            int x = 0;
            int y = 0;
            player = new Player(Game, new Vector2(x * 60, y * 40), this);
            while (!reader.EndOfStream)
            {
                switch (currentTile)
                {
                    case 'w':
                        // waypoint
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40), Tile.TileType.Waypoint, 0.0f);
                        break;
                    case 'p':
                        // player tile. Spawn the player here
                        tiles[x][y] = new Tile(new Vector2(x * 60, y * 40), Tile.TileType.Player, 0.0f);
                        spawn = new Point(x, y);
                        break;
                    case 'P':
                        // platform
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40), Tile.TileType.Platform, 0.0f);
                        break;
                    case 'W':
                        tiles[x][y] = new Tile(textures["sewerWall"], new Vector2(x * 60.0f, y * 40), Tile.TileType.SewerWall, 0.0f);
                        break;
                    case 'S':
                        tiles[x][y] = new Tile(textures["sewerInterior"], new Vector2(x * 60.0f, y * 40), Tile.TileType.SewerInterior, 0.0f);
                        break;
                    case 'V':
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40.0f), Tile.TileType.Waypoint, 0.0f);
                        waypoints.Add("spinner", new Waypoint(Waypoint.WaypointType.Spinner, player, new Vector2(x * 60, y * 40), 0.0f));
                        vehicle = new Vehicle(Game, player, Vehicle.VehicleType.Spinner, new Vector2(x * 60, y * 40), this);
                        vehicleSpawn = new Point(x, y);
                        break;
                    case '.':
                        // air
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40), Tile.TileType.Air, 0.0f);
                        break;
                    case 'x':
                        // road
                        // swap between the two different textures
                        if (swap)
                            tiles[x][y] = new Tile(textures["road0"], new Vector2(x * 60.0f, y * 40), Tile.TileType.Road, 0.0f);
                        else
                            tiles[x][y] = new Tile(textures["road1"], new Vector2(x * 60.0f, y * 40), Tile.TileType.Road, 0.0f);
                        break;
                    case 'X':
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40), Tile.TileType.Waypoint, 0.0f);
                        waypoints.Add("X", new Waypoint(Waypoint.WaypointType.EndRide, vehicle, new Vector2(x * 60, y * 40), 0.0f));
                        break;
                    case 'c':
                        tiles[x][y] = new Tile(textures["concrete"], new Vector2(x * 60.0f, y * 40), Tile.TileType.Concrete, 0.0f);
                        break;
                    case 's':
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40), Tile.TileType.Waypoint, 0.0f);
                        waypoints.Add("s", new Waypoint(Waypoint.WaypointType.SavePoint, this, new Vector2(x * 60, y * 40), 0.0f));
                        break;
                    case 'e':
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40), Tile.TileType.Waypoint, 0.0f);
                        waypoints.Add("e", new Waypoint(Waypoint.WaypointType.EndLevel, this, new Vector2(x * 60, y * 40), 0.0f));
                        break;
                    case 'E':
                        tiles[x][y] = new Tile(new Vector2(x * 60, y * 40), Tile.TileType.Boss, 0.0f);
                        boss = new Boss(Game, x * 60, y * 40, this);
                        break;
                    case 'B':
                        tiles[x][y] = new Tile(textures["buildingInterior"], new Vector2(x * 60.0f, y * 40), Tile.TileType.BuildingInterior, 0.0f);
                        break;
                    case 'b':
                        tiles[x][y] = new Tile(textures["buildingWall"], new Vector2(x * 60.0f, y * 40), Tile.TileType.BuildingWall, 0.0f);
                        break;
                    case '-':
                        tiles[x][y] = new Tile(textures["buildingPlatform"], new Vector2(x * 60.0f, y * 40), Tile.TileType.Platform, 0.0f);
                        break;
                    case '|':
                        tiles[x][y] = new Tile(textures["ladder"], new Vector2(x * 60.0f, y * 40), Tile.TileType.Ladder, 0.0f);
                        break;
                    case 'l':
                        tiles[x][y] = new Tile(textures["powerline"], new Vector2(x * 60.0f, y * 40.0f), Tile.TileType.Powerline, 0.0f);
                        break;
                    case 'L':
                        tiles[x][y] = new Tile(textures["buildingInterior"], new Vector2(x * 60.0f, y * 40.0f), Tile.TileType.Waypoint, 0.0f);
                        vehicle = new Vehicle(Game, player, Vehicle.VehicleType.Lift, new Vector2(x * 60, (y + 1) * 40), this);
                        waypoints.Add("lift", new Waypoint(Waypoint.WaypointType.Lift, player, new Vector2(x * 60, y * 40), 0.0f));
                        if (waypoints["X"] != null)
                            waypoints["X"].handler = vehicle;
                        vehicleSpawn = new Point(x, y);
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
            player.resetRectangle(spawn);
        }

        /// <summary>
        /// Load the next level
        /// </summary>
        public void nextLevel()
        {
            levelx++;
            vehicle = null;
            LoadContent();
        }
        
        /// <summary>
        /// Move the camera based on player position
        /// </summary>
        /// <param name="viewport">The current viewport</param>
        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.35f;

            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraXPosition + marginWidth;
            float marginRight = cameraXPosition + viewport.Width - marginWidth;
            
            const float TopMargin = 0.5f;
            const float BottomMargin = 0.5f;
            float marginTop = cameraYPosition + viewport.Height * TopMargin;
            float marginBottom = cameraYPosition + viewport.Height - viewport.Height * BottomMargin;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraHMovement = 0.0f;
            float cameraVMovement = 0.0f;

            if (player.BoundingRectangle.Min.X < marginLeft)
                cameraHMovement = player.BoundingRectangle.Min.X - marginLeft;
            else if (player.BoundingRectangle.Max.X > marginRight)
                cameraHMovement = player.BoundingRectangle.Max.X - marginRight;
            if (player.BoundingRectangle.Min.Y < marginTop)
                cameraVMovement = player.BoundingRectangle.Min.Y - marginTop;
            else if (player.BoundingRectangle.Max.Y > marginBottom)
                cameraVMovement = player.BoundingRectangle.Max.Y - marginBottom;

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraWidthPosition = 60.0f * width - viewport.Width;
            float maxCameraHeightPosition = 40.0f * height - viewport.Height;
            cameraXPosition = MathHelper.Clamp(cameraXPosition + cameraHMovement, 0.0f, maxCameraWidthPosition);
            cameraYPosition = MathHelper.Clamp(cameraYPosition + cameraVMovement, 0.0f, maxCameraHeightPosition);
        }
        
        /// <summary>
        /// Handle all waypoint events, using the supplied waypoint data
        /// </summary>
        /// <param name="w">The waypoint that generated the event</param>
        public void handleEvent(Waypoint w)
        {
            if (w.Type == Waypoint.WaypointType.SavePoint)
            {
                if (player.BoundingRectangle.Intersects(w.BoundingRectangle))
                {
                    Point oldSpawn = spawn;
                    Vector2 pos = new Vector2(((float)oldSpawn.X * 60.0f), (float)(oldSpawn.Y * 40.0f));
                    tiles[oldSpawn.X][oldSpawn.Y] = new Tile(pos, Tile.TileType.Air, 0.0f);
                    spawn = new Point((int)w.TileCoords.X, (int)w.TileCoords.Y);
                    // overwrite the waypoint with the player tile to set the new spawn point
                    tiles[spawn.X / 60][spawn.Y / 40] = new Tile(new Vector2((float)spawn.X / 60.0f, (float)spawn.Y / 40.0f), Tile.TileType.Player, 0.0f);
                }
            }
            else if (w.Type == Waypoint.WaypointType.EndLevel)
            {
                if (player.BoundingRectangle.Intersects(w.BoundingRectangle))
                    nextLevel();
            }
        }
    }
}
