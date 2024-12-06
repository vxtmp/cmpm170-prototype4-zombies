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


    // NOTE DO NOT ADJUST HERE. ADJUST IN INSPECTOR IN PLAYMODE.
    // NOTE DO NOT ADJUST HERE. ADJUST IN INSPECTOR IN PLAYMODE.
    private bool DEBUG_CLICK_FLOWMAP = false; // enables click to generate + show flowmap.
                                                   // should be false in final build.
                                                   // Note: HIGH OVERHEAD. DO NOT USE IN FINAL BUILD.
    [SerializeField] private bool DEBUG_FLOW_VOLUME = false;
    [SerializeField] private bool DEBUG_LOG_PERFORMANCE = false;

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject doorPrefab;
    public GameObject zombiePrefab;
    public GameObject startingPointPrefab;
    public HumanManager humanManager;

    private const float TILE_SIZE = 1.0f; // Used by spawnObject() to space out the spawned prefabs.
                                          // Used by getTileX and getTileY to convert world space to grid space.
                                          // KEEP THIS AT 1.0f
                                          // 90% sure it will break something if you change it.

    private bool USE_SOUND_PATHING = true;
    private float PATHING_VOLUME_CUTOFF = .5f; // volume cutoff for sound-based pathing.
    private float VOLUME_FALLOFF = 0.1f; // volume falloff for sound-based pathing.

    // . = floor
    // | = wall
    // d = door
    // s = starting point
    // any other character = empty space (null)
    // h = human spawn point
    // z = zombies x10
    string gridString =
    //"||||||||||||||||||||||||||||\n" +
    //"|..........................|\n" +
    //"|........|.................|\n" +
    //"|...||||||.................|\n" +
    //"|...|......................|\n" +
    //"|..........|||||||.........|\n" +
    //"|..........|.....|.........|\n" +
    //"|.|........|.h...|.........|\n" +
    //"|.|........||d||||.........|\n" +
    //"|.|........................|\n" +
    //"|.|........................|\n" +
    //"|.|................zzz.....|\n" +
    //"|..................zzz.....|\n" +
    //"|..................zzz.....|\n" +
    //"|s.........................|\n" +
    //"||||||||||||||||||||||||||||";
    "||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||\n" +
    "|.|..............|....|..........h...|....|..............|.......|\n" +
    "|.|....h.........|....|.....h........|....|..........h...|.......|\n" +
    "|.|..............|....|..............|....|..............|....|..|\n" +
    "|.||||d||||..||..|....||||||||dd||||||...||||d||||||||||d|...|||||\n" +
    "|.........|......|............|...z..|............|......|.......|\n" +
    "|.........|......|..z............................................|\n" +
    "||||......||||d|||...............................................|\n" +
    "|.........|......|.....................z..............z..........|\n" +
    "|.........|......................................................|\n" +
    "|............................................z..........||||||||||\n" +
    "|..................................z....................d........|\n" +
    "|.......................................................|........|\n" +
    "|.......z...............................................|....h...|\n" +
    "|....................z..................................||||||||||\n" +
    "|......z.........................................................|\n" +
    "|.........................................................z......|\n" +
    "|.................................s.............z................|\n" +
    "|..............||||||||..........................................|\n" +
    "|.............|.......|.......||||||||||.........................|\n" +
    "|............|...h...|...........................................|\n" +
    "|...........|.......|..............z........................z....|\n" +
    "|..........d.......|.............................................|\n" +
    "|..........||||||||...............................z..............|\n" +
    "|................................................................|\n" +
    "|.............z.........z........................................|\n" +
    "|.....................................|||d||||||.................|\n" +
    "|.....................................|........|||||||...........|\n" +
    "|.....................................|...h..........|...........|\n" +
    "|.................................z...|............|||||||||.....|\n" +
    "|..........z..........................|...........|...h....|.....|\n" +
    "|s....................................|...........d........|.....|\n" +
    "||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||";

    public class Cell
    {
        private GameObject terrainObject; // the actual object underneath.
        public float pathingWeight; // calc by distance by default recalcFlowmapWeights() function.
        public float distanceFromOrigin; // distance again, but used for volume function to avoid influencing
                                         // the pathingWeight.      
        public float pathingVolume;
        public bool visitedByCurrentBSF;
        public Vector2 position; // center of cell in worldspace.
        public Cell(GameObject terrainPrefab,
                    int x, int y)
        {
            this.terrainObject = Instantiate(terrainPrefab, new Vector3(x * TILE_SIZE + TILE_SIZE / 2, y * TILE_SIZE + TILE_SIZE / 2, 0), Quaternion.identity);
            // initialize pathingValue to max int value
            this.pathingWeight = float.MaxValue;
            this.pathingVolume = 0.0f;
            this.visitedByCurrentBSF = false;
            this.position = GridManager.Instance.getTileCenterFromIndices(x, y);
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
    public List<List<Cell>> grid;
    private Vector2 startingPoint;

    // create a bool flag to use as a MUTEX
    // when there is a recalcFlowmap operation going on, the flag will be set
    private bool FLOWMAP_RECALC_IN_PROGRESS = false;
    // create a queue to store subsequent operation calls. it will need to store parameters for the functions too
    private Queue<Vector2> pathingQueue_Pos = new Queue<Vector2>();
    private Queue<float> pathingQueue_Volume = new Queue<float>(); // combine the two queues into just one queue using pair object


    // Grid             gridString
    // 4                0 1 2 3 4
    // 3                1
    // 2                2
    // 1                3
    // 0 1 2 3 4        4

    // Grid array aligned to represent unity world space (when multiplied by TILE_SIZE) 
    // gridString is read in from top to bottom (due to code)
    private void generateGrid(string gridString)
    {
        string[] rows = gridString.Split('\n');
        grid = new List<List<Cell>>();
        for (int i = 0; i < rows.Length; i++)   // 0 to rows length (top to bottom of gridString)
        {
            //Debug.Log("string: " + rows[i] + " flippedY: " + (rows.Length - 1 - i));
            int flippedY = rows.Length - 1 - i;
            char[] cells = rows[flippedY].ToCharArray();  // split the row into cells
            List<Cell> row = new List<Cell>();      // initialize a new row for grid.
            for (int j = 0; j < cells.Length; j++)
            {
                Cell newCell;
                switch (cells[j])
                {
                    case '.':
                        newCell = new Cell(floorPrefab,
                                              j, i);
                        break;
                    case '|':
                        newCell = new Cell(wallPrefab,
                                              j, i);
                        break;
                    case 's':
                        newCell = new Cell(startingPointPrefab,
                                              j, i);
                        // put the player at the starting point
                        GameManager.Instance.getPlayer().transform.position = new Vector3(
                            j * TILE_SIZE + TILE_SIZE / 2,
                            i * TILE_SIZE + TILE_SIZE / 2,
                            0);
                        startingPoint = new Vector2(j * TILE_SIZE, i * TILE_SIZE);
                        break;
                    case 'h':
                        newCell = new Cell(floorPrefab,
                                              j, i);
                        humanManager.InstantiateHuman(j * TILE_SIZE + TILE_SIZE / 2,
                            i * TILE_SIZE + TILE_SIZE / 2);
                        break;
                    case 'd':
                        newCell = new Cell(floorPrefab, j, i);
                        Instantiate(doorPrefab, new Vector3(j * TILE_SIZE + TILE_SIZE / 2, i * TILE_SIZE + TILE_SIZE / 2, 0), Quaternion.identity);
                        break;
                    case 'z':
                        newCell = new Cell(floorPrefab, j, i);
                        Instantiate(zombiePrefab, new Vector3(j * TILE_SIZE + TILE_SIZE / 2, i * TILE_SIZE + TILE_SIZE / 2, 0), Quaternion.identity);
                        break;
                    default:
                        newCell = null;
                        break;
                }
                row.Add(newCell);

                if (newCell != null)
                    newCell.childTo(this.gameObject);
            }
            grid.Add(row);
        }
    }

    public Vector2 getDestinationNeighbor(Vector2 position)
    {
        if (USE_SOUND_PATHING)
            return getLoudestNeighbor(position);
        else
            return getLowestNeighbor(position);
    }
    public void recalcPathing(Vector2 worldSpacePos, float volume = 50.0f)
    {
        if (FLOWMAP_RECALC_IN_PROGRESS)
        {
            pathingQueue_Pos.Enqueue(worldSpacePos);
            pathingQueue_Volume.Enqueue(volume);
            return;
        }
        FLOWMAP_RECALC_IN_PROGRESS = true;
        if (USE_SOUND_PATHING)
            StartCoroutine(recalcFlowmapVolume(worldSpacePos, volume));
        else
            StartCoroutine(recalcFlowmapWeights(worldSpacePos));
    }
    private Vector2 getLoudestNeighbor(Vector2 position)
    {
        int x = getTileX(position);
        int y = getTileY(position);
        float highestVolume = float.MinValue;
        Cell highestCell = null;
        Vector2[] allNeighbors = combineArray(getNeighbors(position), getDiagonalNeighbors(position));

        foreach (Vector2 neighbor in allNeighbors)
        {
            int nx = (int)neighbor.x;
            int ny = (int)neighbor.y;
            if (grid[ny][nx].pathingVolume > highestVolume)
            {
                highestVolume = grid[ny][nx].pathingVolume;
                highestCell = grid[ny][nx];
            }
        }
        return highestCell.position;
    }
    private Vector2 getLowestNeighbor(Vector2 position)
    {
        int x = getTileX(position);
        int y = getTileY(position);
        // iterate over the four adjacent cells if they exist inside the grid
        // return the cell with the lowest pathingWeight
        float lowestWeight = float.MaxValue;
        Cell lowestCell = null;

        Vector2[] allNeighbors = combineArray(getNeighbors(position), getDiagonalNeighbors(position));

        foreach (Vector2 neighbor in allNeighbors)
        {
            int nx = (int)neighbor.x;
            int ny = (int)neighbor.y;
            if (grid[ny][nx].pathingWeight < lowestWeight)
            {
                lowestWeight = grid[ny][nx].pathingWeight;
                lowestCell = grid[ny][nx];
            }
        }
        return lowestCell.position;
    }

    private void initializeFlowmapWeights()
    {
        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[i].Count; j++)
            {
                grid[i][j].pathingWeight = -1;
                grid[i][j].pathingVolume = 0.0f;
                if (getCellType(j, i) == "Wall" || getCellType(j, i) == "Spikes")
                {
                    grid[i][j].pathingWeight = int.MaxValue;
                }
            }
        }
    }

    private IEnumerator recalcFlowmapWeights(Vector2 worldSpacePos)
    {
        float startTime = Time.realtimeSinceStartup;
        // breadth first search of grid starting at origin point x, y
        // set pathingWeight of each cell to the distance from x, y
        // initialize all pathingWeights to -1 to indicate unvisited.
        string debugMessage = "DEBUG: recalcFlowmapAdditive at: " + worldSpacePos + "\n";

        int x = getTileX(worldSpacePos);
        int y = getTileY(worldSpacePos);
        debugMessage += "Grid Indices: x" + x + " y" + y + "\n";

        // O(n). Reset all visited flags in grid.
        resetVisited();

        // make a queue of cells to visit.
        // Vector2s represent grid indices, not world space.
        Queue<Vector2> queue = new Queue<Vector2>();
        queue.Enqueue(new Vector2(x, y));
        grid[y][x].pathingWeight = 0;

        while (queue.Count > 0)
        {
            Vector2 current = queue.Dequeue();
            float distance = grid[(int)current.y][(int)current.x].pathingWeight;
            debugMessage += "Dequeued: " + current + "\n";
            // visit all neighbors (up down left right) but not diagonals
            foreach (Vector2 neighbor in getNeighbors(current))
            {
                debugMessage += "Checking neighbor: " + neighbor + "\n";
                int nx = (int)neighbor.x;
                int ny = (int)neighbor.y;

                if (isInvalidPath(nx, ny))
                {
                    debugMessage += "failed check at neighbor: " + nx + " " + ny + "\n";
                    continue; // neighbor bounds check.
                }
                grid[ny][nx].visitedByCurrentBSF = true;
                debugMessage += "checks passed.\n";

                if (tileIsEnemyPathable(nx, ny))               // not wall. destructibles should be set to pathable.
                {
                    grid[ny][nx].pathingWeight = distance + 1.0f; // set tile distance
                    queue.Enqueue(neighbor);                   // add to queue
                }
                else
                {
                    grid[ny][nx].pathingWeight = int.MaxValue; // set to max value to indicate impassable
                }
            }
            // visit all diagonal neighbors (incremented by 1.41f (root2))
            foreach (Vector2 neighbor in getDiagonalNeighbors(current))
            {
                debugMessage += "Checking diagonal neighbor: " + neighbor + "\n";
                int nx = (int)neighbor.x;
                int ny = (int)neighbor.y;

                if (isInvalidPath(nx, ny))
                {
                    debugMessage += "failed check at neighbor: " + nx + " " + ny + "\n";
                    continue; // neighbor bounds check.
                }
                grid[ny][nx].visitedByCurrentBSF = true;

                debugMessage += "checks passed.\n";
                if (tileIsEnemyPathable(nx, ny))               // not wall. destructibles should be set to pathable.
                {
                    grid[ny][nx].pathingWeight = distance + 1.41f; // set tile distance
                    queue.Enqueue(neighbor);                   // add to queue
                }
                else
                {
                    grid[ny][nx].pathingWeight = int.MaxValue; // set to max value to indicate impassable
                }
            }
            yield return null;
        }
        if (DEBUG_LOG_PERFORMANCE)
        {
            Debug.Log(debugMessage);
            float endTime = Time.realtimeSinceStartup;
            Debug.Log("Time to calculate flowmap: " + (endTime - startTime) + " seconds. Queue size: " + pathingQueue_Pos.Count);
        }

        FLOWMAP_RECALC_IN_PROGRESS = false;
        if (pathingQueue_Pos.Count > 0)
        {
            Vector2 nextPos = pathingQueue_Pos.Dequeue();
            float nextVolume = pathingQueue_Volume.Dequeue();
            recalcPathing(nextPos, nextVolume);
        }
    }

    private void resetDebugPool()
    {
        if (debugPool != null)
        {
            foreach (GameObject square in debugPool)
            {
                Destroy(square);
            }
        }
        else
        {
            debugPool = new List<GameObject>();
        }
    }

    private IEnumerator recalcFlowmapVolume(Vector2 worldSpacePos, float originVolume)
    {
        float startTime = Time.realtimeSinceStartup;
        // breadth first search of grid starting at origin point x, y
        // set pathingWeight of each cell to the distance from x, y
        // initialize all pathingWeights to -1 to indicate unvisited.
        string debugMessage = "DEBUG: recalcFlowmapReductive at: " + worldSpacePos + "\n";

        int x = getTileX(worldSpacePos);
        int y = getTileY(worldSpacePos);
        debugMessage += "Grid Indices: x" + x + " y" + y + "\n";

        if (DEBUG_FLOW_VOLUME)
        {
            resetDebugPool();
        }

        // O(n). Reset all visited flags in grid.
        resetVisited();

        // make a queue of cells to visit.
        // Vector2s represent grid indices, not world space.
        Queue<Vector2> queue = new Queue<Vector2>();
        queue.Enqueue(new Vector2(x, y));
        grid[y][x].distanceFromOrigin = 0.1f;

        while (queue.Count > 0)
        {
            Vector2 current = queue.Dequeue();
            float distance = grid[(int)current.y][(int)current.x].distanceFromOrigin;
            debugMessage += "Dequeued: " + current + "\n";
            // visit all neighbors (up down left right) but not diagonals
            foreach (Vector2 neighbor in getNeighbors(current))
            {
                debugMessage += "Checking neighbor: " + neighbor + "\n";
                int nx = (int)neighbor.x;
                int ny = (int)neighbor.y;

                if (isInvalidPath(nx, ny))
                {
                    debugMessage += "failed check at neighbor: " + nx + " " + ny + "\n";
                    continue; // neighbor bounds check.
                }
                grid[ny][nx].visitedByCurrentBSF = true;

                debugMessage += "checks passed.\n";
                if (tileIsEnemyPathable(nx, ny))               // not wall. destructibles should be set to pathable.
                {
                    grid[ny][nx].distanceFromOrigin = distance + 1.0f; // set tile distance
                    float currentSoundVolume = originVolume / ((distance + 1.0f) *
                                                                  (distance + 1.0f));
                    if (currentSoundVolume > PATHING_VOLUME_CUTOFF)
                    {
                        grid[ny][nx].pathingVolume += currentSoundVolume;
                        queue.Enqueue(neighbor);                   // add to queue
                    }
                }
                else
                {
                    grid[ny][nx].pathingVolume = 0.0f;
                    grid[ny][nx].distanceFromOrigin = float.MaxValue; // set to max value to indicate impassable
                }
                if (DEBUG_FLOW_VOLUME)
                {
                    spawnDebugSquare(nx, ny, grid[ny][nx].pathingVolume);
                }
            }
            // visit all diagonal neighbors (incremented by 1.41f (root2))
            foreach (Vector2 neighbor in getDiagonalNeighbors(current))
            {
                debugMessage += "Checking diagonal neighbor: " + neighbor + "\n";
                int nx = (int)neighbor.x;
                int ny = (int)neighbor.y;

                if (isInvalidPath(nx, ny))
                {
                    debugMessage += "failed check at neighbor: " + nx + " " + ny + "\n";
                    continue; // neighbor bounds check.
                }

                debugMessage += "checks passed.\n";
                if (tileIsEnemyPathable(nx, ny))               // not wall. destructibles should be set to pathable.
                {
                    grid[ny][nx].distanceFromOrigin = distance + 1.41f;       // set tile distance
                    float currentSoundVolume = originVolume / ((distance + 1.41f) *
                                                                  (distance + 1.41f));
                    if (currentSoundVolume > PATHING_VOLUME_CUTOFF)
                    {
                        grid[ny][nx].visitedByCurrentBSF = true;
                        grid[ny][nx].pathingVolume += currentSoundVolume;
                        queue.Enqueue(neighbor);                   // add to queue
                    }
                }
                else
                {
                    grid[ny][nx].visitedByCurrentBSF = true;
                    grid[ny][nx].distanceFromOrigin = float.MaxValue; // set to max value to indicate impassable
                }
                if (DEBUG_FLOW_VOLUME)
                {
                    spawnDebugSquare(nx, ny, grid[ny][nx].pathingVolume);
                }
            }
            yield return null;
        }

        if (DEBUG_LOG_PERFORMANCE)
        {
            Debug.Log(debugMessage);
            float endTime = Time.realtimeSinceStartup;
            Debug.Log("Time to calculate flowmap: " + (endTime - startTime) + " seconds. Queue size: " + pathingQueue_Pos.Count);
        }

        FLOWMAP_RECALC_IN_PROGRESS = false;
        if (pathingQueue_Pos.Count > 0)
        {
            Vector2 nextPos = pathingQueue_Pos.Dequeue();
            float nextVolume = pathingQueue_Volume.Dequeue();
            recalcPathing(nextPos, nextVolume);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        generateGrid(gridString);
        initializeFlowmapWeights();
        if (DEBUG_CLICK_FLOWMAP)
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
        return mousePos;
    }

    // mouseclick event handler
    public void DebugMap()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 coords = getMouseCoords();
            if (isValidOrigin(coords))
            {
                recalcFlowmapWeights(coords);
                spawnDebugSquares();
            }
        }
    }

    public void spawnDebugSquares()
    {
        foreach (GameObject square in debugPool)
        {

            Destroy(square);
        }
        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[i].Count; j++)
            {
                if (grid[i][j] != null)
                {
                    spawnDebugSquare(j, i, grid[i][j].pathingWeight);

                }
            }
        }
    }

    [SerializeField] private float DEBUG_HUE_DEFAULT0POINT7 = 0.7f;
    [SerializeField] private float DEBUG_DISTANCE_DEFAULT30 = 30f;
    public void spawnDebugSquare(int x, int y, float distance)
    {
        Vector2 tileCenter = getTileCenterFromIndices(x, y);
        GameObject square = Instantiate(debugSquare, tileCenter, Quaternion.identity);
        // set parent of square to this
        square.transform.parent = this.transform;
        debugPool.Add(square);
        SpriteRenderer renderer = square.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // Calculate color based on distance
            //float brightness = distance >= 0 && distance < int.MaxValue ? Mathf.Clamp01(1f - (distance / 20f)) : 0;
            //Color color = new Color(brightness, brightness, brightness); // Set color with the same brightness for all channels

            // Set hue based on distance. If distance is closer, make it more red. If distance is farther, make it more blue.
            float hue = distance >= 0 && distance < int.MaxValue ? Mathf.Clamp01(DEBUG_HUE_DEFAULT0POINT7 - (distance / DEBUG_DISTANCE_DEFAULT30)) : 0;
            Color color = Color.HSVToRGB(hue, 1, 1);

            // Set the color of the material
            renderer.material.color = color;
        }
    }

    public void debugGridPrint()
    {
        string gridString = "Grid:\n";
        // print a formatted 2d string according to grid
        for (int i = 0; i < grid.Count; i++)
        {
            string row = "" + i + ":";
            for (int spaces = 0; spaces < 2 - i.ToString().Length; spaces++)
            {
                row += " ";
            }
            for (int j = 0; j < grid[i].Count; j++)
            {
                if (grid[i][j] != null)
                {
                    char terrainTypeAbbreviated = grid[i][j].getTerrainType()[0];
                    row += terrainTypeAbbreviated;
                }
                else
                {
                    row += " ";
                }
            }
            gridString += row + "\n";
        }
        Debug.Log(gridString);
    }

    // Make a DebugVolume() function that will perform a BSF on a given position
    // and copy the logic from spawnDebugSquare except it will grab the pathingVolume
    // instead of the pathingWeight.
    private void DebugVolume(Vector2 position)
    {

    }

    // ------------------------------------
    // Helper functions
    // ------------------------------------

    private void resetVisited()
    {
        // row # (y)
        for (int i = 0; i < grid.Count; i++)
        {   // column # (x)
            for (int j = 0; j < grid[i].Count; j++)
            {
                if (grid[i][j] != null)
                {
                    grid[i][j].visitedByCurrentBSF = false;
                    grid[i][j].distanceFromOrigin = float.MaxValue;
                    grid[i][j].pathingVolume *= VOLUME_FALLOFF;
                }
            }
        }
    }

    private bool isInvalidPath(int nx, int ny)
    {
        if (nx < 0 || nx >= grid[ny].Count || ny < 0 || ny >= grid.Count || // array bounds check
            grid[ny][nx] == null ||                                         // null check
            grid[ny][nx].visitedByCurrentBSF == true)                       // visited check
        {
            return true;
        }
        return false;
    }
    public bool isValidOrigin(Vector2 worldSpacePos)
    {
        int x = getTileX(worldSpacePos);
        int y = getTileY(worldSpacePos);

        // if the coordinate is within the grid, and is not null, wall, or spike, return true;
        if (y >= 0 && y < grid.Count)
        {
            if (x >= 0 && x < grid[y].Count)
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

    public Vector2[] combineArray(Vector2[] a1, Vector2[] a2)
    {
        Vector2[] combined = new Vector2[a1.Length + a2.Length];
        a1.CopyTo(combined, 0);
        a2.CopyTo(combined, a1.Length);
        return combined;
    }
    public int getTileX(Vector2 coords)
    {
        return (int)(coords.x / TILE_SIZE);
    }
    public int getTileY(Vector2 coords)
    {
        return (int)(coords.y / TILE_SIZE);
    }

    public Vector2 getTileCenterFromIndices(int x, int y)
    {
        return new Vector2(x * TILE_SIZE + TILE_SIZE / 2, y * TILE_SIZE + TILE_SIZE / 2);
    }
    private bool tileIsEnemyPathable(int x, int y)
    {
        if (grid[y][x] == null)
        {
            return false;
        }
        if (grid[y][x].getTerrainType() == "Wall" || grid[y][x].getTerrainType() == "Spikes")
        {
            return false;
        }
        return true;
    }
    public Vector2 getStartingPoint()
    {
        return startingPoint;
    }

    public float getTileSize()
    {
        return TILE_SIZE;
    }
    public string getCellType(int x, int y)
    {
        return grid[y][x].getTerrainType();
    }

    public Vector2 getTile(Vector2 coords)
    {
        return new Vector2(getTileX(coords), getTileY(coords));
    }
    public Vector2[] getNeighbors(Vector2 position)
    {
        int x = getTileX(position);
        int y = getTileY(position);
        List<Vector2> neighbors = new List<Vector2>();
        if (y > 0)
        {
            neighbors.Add(new Vector2(x, y - 1));
        }
        if (y < grid.Count - 1)
        {
            neighbors.Add(new Vector2(x, y + 1));
        }
        if (x > 0)
        {
            neighbors.Add(new Vector2(x - 1, y));
        }
        if (x < grid[y].Count - 1)
        {
            neighbors.Add(new Vector2(x + 1, y));
        }
        return neighbors.ToArray();
    }
    public Vector2[] getDiagonalNeighbors(Vector2 position)
    {
        int x = getTileX(position);
        int y = getTileY(position);
        List<Vector2> neighbors = new List<Vector2>();
        if (y > 0)
        {
            if (x > 0)
            {
                // check for path down or left to prevent diagonal leapfrogging
                if (tileIsEnemyPathable(x - 1, y) || tileIsEnemyPathable(x, y - 1))
                {
                    neighbors.Add(new Vector2(x - 1, y - 1)); // add bottom left
                }
            }
            if (x < grid[y].Count - 1)
            {
                // check for path down or right to prevent diagonal leapfrogging
                if (tileIsEnemyPathable(x + 1, y) || tileIsEnemyPathable(x, y - 1))
                {
                    neighbors.Add(new Vector2(x + 1, y - 1)); // add bottom right
                }
            }
        }
        if (y < grid.Count - 1)
        {
            if (x > 0)
            {
                // check for path up or left to prevent diagonal leapfrogging
                if (tileIsEnemyPathable(x - 1, y) || tileIsEnemyPathable(x, y + 1))
                {
                    neighbors.Add(new Vector2(x - 1, y + 1)); // add top left
                }
            }
            if (x < grid[y].Count - 1)
            {
                // check for path up or right to prevent diagonal leapfrogging
                if (tileIsEnemyPathable(x + 1, y) || tileIsEnemyPathable(x, y + 1))
                {
                    neighbors.Add(new Vector2(x + 1, y + 1)); // add top right
                }
            }
        }
        return neighbors.ToArray();
    }

    public Vector2[] getViableNeighbors(Vector2 position)
    {
        Vector2[] neighbors = getNeighbors(position);
        List<Vector2> vNeighbors = new List<Vector2>();

        for (int i = 0; i < neighbors.Length; i++)
        {
            if (tileIsEnemyPathable((int)neighbors[i].x, (int)neighbors[i].y) && !isInvalidPath((int)neighbors[i].x, (int)neighbors[i].y))
            {
                //Debug.Log(neighbors[i]);
                vNeighbors.Add(neighbors[i]);
            }
        }
        return vNeighbors.ToArray();
    }
}
