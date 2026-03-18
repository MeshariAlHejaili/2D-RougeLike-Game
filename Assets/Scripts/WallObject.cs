using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    [Header("Visuals")]
    [SerializeField] 
    [Tooltip("The tile sprite displayed while this wall is active.")]
    private Tile m_ObstacleTile;

    [Header("Stats")]
    [SerializeField] 
    [Range(1, 10)]
    [Tooltip("How many times the player must hit the wall to destroy it.")]
    private int m_MaxHealth = 3;

    private int m_CurrentHealth;
    private Tile m_OriginalTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        // Initialize health state
        m_CurrentHealth = m_MaxHealth;

        // Save the background tile and swap it for the wall tile
        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, m_ObstacleTile);
    }

public override bool PlayerWantsToEnter()
{
    m_CurrentHealth -= 1;
    GameManager.Instance.PlayerController.Attack();
    
    if (m_CurrentHealth > 0) return false;

    GameManager.Instance.PlayVFX(GameManager.Instance.WallDestroyPrefab, transform.position);
    GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);
    
    // FIX: Tell the board this cell no longer contains an object
    GameManager.Instance.BoardManager.GetCellData(m_Cell).ContainedObject = null;
    
    PoolManager.Instance.ReturnToPool(gameObject);
    return true;
}
}