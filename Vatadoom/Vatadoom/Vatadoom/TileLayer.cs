using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class TileLayer
    {
        private Tile[][] tiles;
        private int width;
        private int height;
        public int depth { get; private set; } // constant, set by the tile map file
        private List<Transition> transitions;
        private Dictionary<String, Texture2D> textures;
        private Vehicle vehicle;
        private List<Waypoint> waypoints;
        private Point vehicleSpawn;
        private Level owner;
        private int layerId;

        public TileLayer()
        {
        }

        public void Initialize()
        {
        }

        public void Update()
        {
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, int left, int right, int bottom, int top)
        {
            for (int i = left; i <= right; i++)
                for (int j = top; j <= bottom; j++)
                    if (tiles[i][j] != null)
                        tiles[i][j].Draw(spriteBatch);
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
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40), Tile.TileType.Waypoint, 0.0f);
                        break;
                    case 'p':
                        // player tile. Spawn the player here
                        tiles[x][y] = new Tile(new Vector2(x * 60, y * 40), Tile.TileType.Player, 0.0f);
                        owner.spawn = new Point(x, y);
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
                        waypoints.Add(new Waypoint(Waypoint.WaypointType.Spinner, owner.player, new Vector2(x, y), 0.0f));
                        vehicle = new Vehicle(owner.Game, owner.player, Vehicle.VehicleType.Spinner, new Vector2(x * 60, y * 40), owner);
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
                        waypoints.Add(new Waypoint(Waypoint.WaypointType.EndRide, vehicle, new Vector2(x, y), 0.0f));
                        break;
                    case 'c':
                        tiles[x][y] = new Tile(textures["concrete"], new Vector2(x * 60.0f, y * 40), Tile.TileType.Concrete, 0.0f);
                        break;
                    case 's':
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40), Tile.TileType.Waypoint, 0.0f);
                        waypoints.Add(new Waypoint(Waypoint.WaypointType.SavePoint, owner, new Vector2(x, y), 0.0f));
                        break;
                    case 'e':
                        tiles[x][y] = new Tile(new Vector2(x * 60.0f, y * 40), Tile.TileType.Waypoint, 0.0f);
                        waypoints.Add(new Waypoint(Waypoint.WaypointType.EndLevel, owner, new Vector2(x, y), 0.0f));
                        break;
                    case 'E':
                        tiles[x][y] = new Tile(new Vector2(x * 60, y * 40), Tile.TileType.Boss, 0.0f);
                        owner.boss = new Boss(owner.Game, x * 60, y * 40, owner);
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
                        vehicle = new Vehicle(owner.Game, owner.player, Vehicle.VehicleType.Lift, new Vector2(x * 60, (y + 1) * 40), owner);
                        waypoints.Add(new Waypoint(Waypoint.WaypointType.Lift, owner.player, new Vector2(x * 60, y * 40), 0.0f));
                        //if (waypoints["X"] != null)
                            //waypoints["X"].handler = vehicle;
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
        }
        public bool containsTransition(int id)
        {
            for (int i = 0; i < transitions.Count; i++)
                if (transitions[i].id == id && transitions[i].currentLayer != layerId)
                    return true;
            return false;
        }
    }
}
