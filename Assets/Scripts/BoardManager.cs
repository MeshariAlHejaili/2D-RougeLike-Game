using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    // Nested class to store state for each individual grid cell
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    [Header("Board Dimensions")]
    [SerializeField] [Range(5, 25)] [Tooltip("Number of columns in the game grid.")]
    private int m_Width = 8;

    [SerializeField] [Range(5, 25)] [Tooltip("Number of rows in the game grid.")]
    private int m_Height = 8;

    [Header("Tile Assets")]
    [SerializeField] [Tooltip("Array of tiles used for walkable ground.")]
    private Tile[] m_GroundTiles;

    [SerializeField] [Tooltip("Array of tiles used for the indestructible outer boundary.")]
    private Tile[] m_WallTiles;

    [Header("Object Prefabs")]
    [SerializeField] [Tooltip("The prefab used for destructible walls.")]
    private WallObject m_WallPrefab;

    [SerializeField] [Tooltip("The prefab used for the level exit.")]
    private ExitCellObject m_ExitCellPrefab;

    [SerializeField] [Tooltip("The prefab used for enemies.")]
    private Enemy m_EnemyPrefab;

    [SerializeField] [Tooltip("Array of various food prefabs to spawn.")]
    private FoodObject[] m_FoodPrefabs;

    // Private references and state
    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;

    // Public getters for other scripts to access dimensions
    public int Width => m_Width;
    public int Height => m_Height;

    public void Init()
    {
        //  DYNAMIC SCALING MATH
        int level = GameManager.Instance.CurrentLevel;
        
        // Increase board size every 2 levels, cap at 20x20
        m_Width = Mathf.Min(20, 8 + (level / 2)); 
        m_Height = Mathf.Min(20, 8 + (level / 2));
        
        // 1. Prepare Object Pools
        foreach (var prefab in m_FoodPrefabs) PoolManager.Instance.PrewarmPool(prefab.gameObject, 5);
        PoolManager.Instance.PrewarmPool(m_WallPrefab.gameObject, 10);
        PoolManager.Instance.PrewarmPool(m_EnemyPrefab.gameObject, 5);

        // 2. Component Setup
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        m_EmptyCellsList = new List<Vector2Int>();
        
        m_BoardData = new CellData[m_Width, m_Height];

        // 3. Procedural Grid Generation
        for (int y = 0; y < m_Height; ++y)
        {
            for (int x = 0; x < m_Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();
                
                // Border Detection: Top, Bottom, Left, or Right edges
                if (x == 0 || y == 0 || x == m_Width - 1 || y == m_Height - 1)
                {
                    tile = m_WallTiles[Random.Range(0, m_WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = m_GroundTiles[Random.Range(0, m_GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    
                    // Track this cell as a valid spawn location
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }
                
                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        
        // 4. Reserve critical cells
        m_EmptyCellsList.Remove(new Vector2Int(1, 1)); // Player Start

        Vector2Int endCoord = new Vector2Int(m_Width - 2, m_Height - 2);
        AddObject(Instantiate(m_ExitCellPrefab), endCoord); // Level Exit
        m_EmptyCellsList.Remove(endCoord);

        // 5. Populate the board
        GenerateWall();
        GenerateEnemies();
        GenerateFood();
        AdjustCamera();
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= m_Width || cellIndex.y < 0 || cellIndex.y >= m_Height)
        {
            return null;
        }
        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    void GenerateWall()
    {
        // Increase walls as levels progress
        int level = GameManager.Instance.CurrentLevel;
        int wallCount = Random.Range(5, 10) + level;
        for (int i = 0; i < wallCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            GameObject wallObj = PoolManager.Instance.Get(m_WallPrefab.gameObject);
            AddObject(wallObj.GetComponent<WallObject>(), coord);
        }
    }

    void GenerateFood()
    {
        int foodCount = 5;
        for (int i = 0; i < foodCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            FoodObject randomPrefab = m_FoodPrefabs[Random.Range(0, m_FoodPrefabs.Length)];
            
            // REPLACED INSTANTIATE WITH POOLER
            GameObject foodObj = PoolManager.Instance.Get(randomPrefab.gameObject);
            AddObject(foodObj.GetComponent<FoodObject>(), coord);
        }
    }

    void GenerateEnemies()
    {
        // Increase enemies every 2 levels
        int level = GameManager.Instance.CurrentLevel;
        int enemyCount = 1 + (level / 2);

        for (int i = 0; i < enemyCount; i++)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            // REPLACED INSTANTIATE WITH POOLER
            GameObject enemyObj = PoolManager.Instance.Get(m_EnemyPrefab.gameObject);
            AddObject(enemyObj.GetComponent<Enemy>(), coord);
        }
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    public void Clean()
    {
        if (m_BoardData == null) return;

        for (int y = 0; y < m_Height; ++y)
        {
            for (int x = 0; x < m_Width; ++x)
            {
                var cellData = m_BoardData[x, y];
                if (cellData.ContainedObject != null)
                {
                    // REPLACED DESTROY WITH RECYCLING
                    PoolManager.Instance.ReturnToPool(cellData.ContainedObject.gameObject);
                    cellData.ContainedObject = null;
                }
                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }

    private void AdjustCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // Find grid center coordinates
        float centerX = (m_Width - 1) / 2f;
        float centerY = (m_Height - 1) / 2f;

        // Convert to world space and move camera (Z = -10)
        Vector3 centerWorldPos = m_Grid.GetCellCenterWorld(new Vector3Int(Mathf.FloorToInt(centerX), Mathf.FloorToInt(centerY), 0));
        mainCam.transform.position = new Vector3(centerWorldPos.x, centerWorldPos.y, -10);

        // Calculate vertical and horizontal zoom with padding
        float verticalSize = (m_Height / 2f) + 2f;
        float horizontalSize = ((m_Width / 2f) + 2f) / mainCam.aspect;

        // Apply the larger size to ensure the whole board fits
        mainCam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
}