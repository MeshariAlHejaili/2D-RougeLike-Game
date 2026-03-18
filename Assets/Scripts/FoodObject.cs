using UnityEngine;

public class FoodObject : CellObject
{
    [Header("Settings")]
    [SerializeField] 
    [Range(1, 50)]
    [Tooltip("The amount of food energy restored to the player upon collection.")]
    private int m_AmountGranted = 10;

    // Triggered when the player successfully moves into this cell
    public override void PlayerEntered()
    {
        // Play visual and audio feedback
        GameManager.Instance.PlayVFX(GameManager.Instance.FoodCollectPrefab, transform.position);
        GameManager.Instance.PlaySfx(GameManager.Instance.FoodSound);
        
        // Recycle this object back into the pool
        PoolManager.Instance.ReturnToPool(gameObject);
        
        // Update the player's food resources
        GameManager.Instance.ChangeFood(m_AmountGranted);
    }
}