using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    // Singleton
    public static GridManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public const bool DEBUG_SHOW_ALL = true; // initializes all fog objects to inactive for debug purposes.
                                              // should be false in final build.
    public const bool DEBUG_CLICK_FLOWMAP = false; // enables click to generate + show flowmap.
                                                   // should be false in final build.
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject startingPointPrefab;

    private const float TILE_SIZE = 1.0f; // Used by spawnObject() to space out the spawned prefabs.

    // . = floor
    // | = wall
    // s = starting point
    // any other character = empty space (null)
    // 
    string gridString =
        "||||||||||||||||||||||\n" +
        "|................|...|\n" +
        "|................|...|\n" +
        "|................|...|\n" +
        "|......||||..||......|\n" +
        "|.........|......|...|\n" +
        "|.........|......|...|\n" +
        "||||......|......|...|\n" +
        "|.........|......|...|\n" +
        "|....|....|..........|\n" +
        "|s...|....|..........|\n" +
        "||||||||||||||||||||||";

    public class Cell
    {
        private GameObject terrainObject; // the actual object underneath.
        public int pathingWeight;
        public Vector2 position;
        public Cell(GameObject terrainPrefab,
                    int x, int y)
        {
            this.terrainObject = Instantiate(terrainPrefab, new Vector3(x * TILE_SIZE, y * TILE_SIZE, 0), Quaternion.identity);
            // initialize pathingValue to max int value
            this.pathingWeight = int.MaxValue;
            this.position = new Vector2(x * TILE_SIZE, y * TILE_SIZE);
        }
        public string getTerrainType()
        {
            return terrainObject.tag;
        }
        public void childTo(GameObject parent)
        {
            terrainObject.transform.parent = parent.transform;
        }
    }
    public Cell[][] grid;
    public Vector2 startingPoint;

    public float getTileSize()
    {
        return TILE_SIZE;
    }
    public string getCellType(int x, int y)
    {
        return grid[y][x].getTerrainType();
    }

    // make some kind of function to parse a string to spawn a grid.

    // Grid             gridString
    // 4                0 1 2 3 4
    // 3                1
    // 2                2
    // 1                3
    // 0 1 2 3 4        4

    // Grid array aligned to represent unity world space. 
    // gridString is read in from top to bottom (due to code)
    public void generateGrid(string gridString)
    {
        string[] rows = gridString.Split('\n');
        grid = new Cell[rows.Length][];
        for (int i = 0; i < rows.Length; i++)   // 0 to rows length (top to bottom of gridString)
        {
            char[] cells = rows[i].ToCharArray();  // split the row into cells
            int flippedY = rows.Length - 1 - i;
            grid[flippedY] = new Cell[cells.Length];      // initialize a new row for grid.
            for (int j = 0; j < cells.Length; j++)
            {
                switch (cells[j])
                {
                    case '.':
                        grid[flippedY][j] = new Cell(floorPrefab,
                                              j, flippedY);
                        break;
                    case '|':
                        grid[flippedY][j] = new Cell(wallPrefab,
                                              j, flippedY);
                        break;
                    case 's':
                        grid[flippedY][j] = new Cell(startingPointPrefab,
                                              j, flippedY);
                        // put the player at the starting point
                        GameManager.Instance.getPlayer().transform.position = new Vector3(j * TILE_SIZE, flippedY * TILE_SIZE, 0);
                        startingPoint = new Vector2(j * TILE_SIZE, flippedY * TILE_SIZE);
                        break;
                    default:
                        grid[flippedY][j] = null;
                        break;
                }
                if (grid[flippedY][j] != null)
                    grid[flippedY][j].childTo(this.gameObject);
            }
        }
    }

    public Vector2 getLowestNeighbor(Vector2 position)
    {
        int x = getTileX(position);
        int y = getTileY(position);
        // iterate over the four adjacent cells if they exist inside the grid
        // return the cell with the lowest pathingWeight
        int lowestWeight = int.MaxValue;
        Cell lowestCell = null;
        if (y > 0)
        {
            if (grid[y - 1][x].pathingWeight < lowestWeight)
            {
                lowestWeight = grid[y - 1][x].pathingWeight;
                lowestCell = grid[y - 1][x];
            }
        }
        if (y < grid.Length - 1)
        {
            if (grid[y + 1][x].pathingWeight < lowestWeight)
            {
                lowestWeight = grid[y + 1][x].pathingWeight;
                lowestCell = grid[y + 1][x];
            }
        }
        if (x > 0)
        {
            if (grid[y][x - 1].pathingWeight < lowestWeight)
            {
                lowestWeight = grid[y][x - 1].pathingWeight;
                lowestCell = grid[y][x - 1];
            }
        }
        if (x < grid[y].Length - 1)
        {
            if (grid[y][x + 1].pathingWeight < lowestWeight)
            {
                lowestWeight = grid[y][x + 1].pathingWeight;
                lowestCell = grid[y][x + 1];
            }
        }
        return lowestCell.position;
    }

    public Vector2 getTile(Vector2 coords)
    {
        return new Vector2(getTileX(coords), getTileY(coords));
    }


    public void initializeFlowmapWeights()
    {
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid[i].Length; j++)
            {
                grid[i][j].pathingWeight = -1;
                if (getCellType(j, i) == "Wall" || getCellType(j, i) == "Spikes")
                {
                    grid[i][j].pathingWeight = int.MaxValue;
                }
            }
        }
    }
    public void recalcFlowmapWeights(int x, int y)
    {
        // breadth first search of grid starting at origin point x, y
        // set pathingWeight of each cell to the distance from x, y

        // initialize all pathingWeights to -1 to indicate unvisited.
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid[i].Length; j++)
            {
                if (grid[i][j] != null)
                    grid[i][j].pathingWeight = -1;
            }
        }

        // make a queue of cells to visit
        Queue<Vector2> queue = new Queue<Vector2>();
        queue.Enqueue(new Vector2(x, y));
        grid[y][x].pathingWeight = 0;

        while (queue.Count > 0)
        {
            Vector2 current = queue.Dequeue();
            int distance = grid[(int)current.y][(int)current.x].pathingWeight;

            //visit all neighbors (up down left right) but not diagonals
            foreach (Vector2 neighbor in new Vector2[] { new Vector2(current.x, current.y + 1),
                                                                    new Vector2(current.x, current.y - 1),
                                                                    new Vector2(current.x + 1, current.y),
                                                                    new Vector2(current.x - 1, current.y)})
            {
                int nx = (int)neighbor.x;
                int ny = (int)neighbor.y;
                if (nx >= 0 && nx < grid[ny].Length && ny >= 0 && ny < grid.Length)
                {
                    if (grid[ny][nx] != null)
                    {
                        if (grid[ny][nx].pathingWeight == -1)       // if unvisited
                        {
                            if (tileIsEnemyPathable(nx, ny))        // and not wall/spike/null
                            {
                                grid[ny][nx].pathingWeight = distance + 1; // set tile distance
                                queue.Enqueue(neighbor);                   // add to queue
                            }
                            else
                            {
                                grid[ny][nx].pathingWeight = int.MaxValue; // set to max value to indicate impassable
                            }
                        }
                    }
                }
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        generateGrid(gridString);
        initializeFlowmapWeights();
        debugPool = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (DEBUG_CLICK_FLOWMAP)
            DebugMap();
    }
    // ------------------------------------
    // Debug helpers
    // ------------------------------------

    public GameObject debugSquare; // prefab
    private List<GameObject> debugPool;

    // get mouse click coordinates
    public Vector2 getMouseCoords()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(Mathf.Floor(mousePos.x), Mathf.Floor(mousePos.y));
    }

    // mouseclick event handler
    public void DebugMap()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 coords = getMouseCoords();
            int tileX = getTileX(coords);
            int tileY = getTileY(coords);
            Debug.Log("Clicked " + coords);
            Debug.Log("X: " + tileX + " Y: " + tileY);
            if (isValidOrigin(tileX, tileY))
            {
                recalcFlowmapWeights(tileX, tileY);
                spawnDebugSquares(tileX, tileY);
            }
        }
    }

    public bool isValidOrigin(int x, int y)
    {
        // if the coordinate is within the grid, and is not null, wall, or spike, return true;
        if (y >= 0 && y < grid.Length)
        {
            if (x >= 0 && x < grid[y].Length)
            {
                if (grid[y][x] != null)
                {
                    if (grid[y][x].getTerrainType() != "Wall" && grid[y][x].getTerrainType() != "Spikes")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void spawnDebugSquares(int x, int y)
    {
        foreach (GameObject square in debugPool)
        {

            Destroy(square);
        }
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid[i].Length; j++)
            {
                if (grid[i][j] != null)
                {
                    spawnDebugSquare(j, i, grid[i][j].pathingWeight);
                }
            }
        }
    }

    public void spawnDebugSquare(int x, int y, int distance)
    {
        Vector2 tileCenter = new Vector2(x, y);
        GameObject square = Instantiate(debugSquare, tileCenter, Quaternion.identity);
        debugPool.Add(square);
        SpriteRenderer renderer = square.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // Calculate color based on distance
            float brightness = distance >= 0 && distance < int.MaxValue ? Mathf.Clamp01(1f - (distance / 20f)) : 0;
            Color color = new Color(brightness, brightness, brightness); // Set color with the same brightness for all channels

            // Set the color of the material
            renderer.material.color = color;
        }
    }

    // ------------------------------------
    // Helper functions
    // ------------------------------------


    public int getTileX(Vector2 coords)
    {
        return (int)(coords.x / TILE_SIZE);
    }
    public int getTileY(Vector2 coords)
    {
        return (int)(coords.y / TILE_SIZE);
    }
    private bool tileIsEnemyPathable(int x, int y)
    {
        if (grid[y][x].getTerrainType() == "Wall" || grid[y][x].getTerrainType() == "Spikes")
        {
            return false;
        }
        return true;
    }
}
