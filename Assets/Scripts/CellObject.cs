using UnityEngine;

public class CellObject : MonoBehaviour
{
    [Header("Grid Data")]
    [SerializeField] 
    [Tooltip("The current grid coordinates of this object on the board.")]
    protected Vector2Int m_Cell;

    // Base initialization to store the object's position on the grid
    public virtual void Init(Vector2Int cell)
    {
        m_Cell = cell;
    }
  
    // Triggered by the PlayerController after the player successfully moves into this cell
    public virtual void PlayerEntered()
    {
        // To be implemented by subclasses (e.g., FoodObject, ExitCellObject)
    }

    // Called before movement to determine if the player is allowed to enter this cell
    // Returns true if the player can enter, false if they are blocked (e.g., by a Wall or Enemy)
    public virtual bool PlayerWantsToEnter()
    {
        return true;
    }
}