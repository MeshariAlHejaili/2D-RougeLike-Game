using UnityEngine;
using UnityEngine.Tilemaps;

public class ExitCellObject : CellObject
{
    [Header("Visuals")]
    [SerializeField] 
    [Tooltip("The unique tile used to visually represent the exit point on the board.")]
    private Tile m_EndTile;

    // Called when the exit is spawned to set its visual tile
    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        
        // Update the tilemap at this coordinate to show the exit sprite
        GameManager.Instance.BoardManager.SetCellTile(coord, m_EndTile);
    }

    // Triggered when the player successfully moves into the exit cell
    public override void PlayerEntered()
    {
        // Advance the game to the next procedurally generated level
        GameManager.Instance.NewLevel();
    }
}