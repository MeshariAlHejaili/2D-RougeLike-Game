using UnityEngine;

public class Enemy : CellObject
{
    [Header("Enemy Stats")]
    [SerializeField] 
    [Range(1, 10)]
    [Tooltip("Total health points. The enemy is recycled when this reaches 0.")]
    private int m_MaxHealth = 3;

    [SerializeField] 
    [Range(1.0f, 15.0f)]
    [Tooltip("The speed at which the enemy visually slides between grid cells.")]
    private float m_MoveSpeed = 5.0f;

    [SerializeField] 
    [Range(1, 20)]
    [Tooltip("The amount of food deducted from the player upon a successful attack.")]
    private int m_FoodDamage = 3;

    [Header("Visual References")]
    [SerializeField]
    [Tooltip("The animator component for movement and attack animations.")]
    private Animator m_Animator;

    // Internal state variables
    private int m_CurrentHealth;
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;
    
    // NEW: Used to remember food when stepping on its cell
    private CellObject m_UnderlyingObject;

    private void OnEnable()
    {
        // Subscribe only when the enemy is active on the board
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick += TurnHappened;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe the moment the enemy is returned to the pool
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick -= TurnHappened;
        }
    }

    // REMOVE the Awake and OnDestroy subscription logic to avoid duplicates
    private void Awake()
    {
        if (m_Animator == null) m_Animator = GetComponent<Animator>();
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = m_MaxHealth;
        m_IsMoving = false;
        m_UnderlyingObject = null; // Ensure we start fresh
    }

    private void Update()
    {
        // Smoothly interpolate the sprite's position during movement
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, m_MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                m_Animator.SetBool("Moving", false);
            }
        }
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth -= 1;
        GameManager.Instance.PlayerController.Attack();

        if (m_CurrentHealth <= 0)
        {
            GameManager.Instance.PlayVFX(GameManager.Instance.EnemyDeathPrefab, transform.position);
            GameManager.Instance.PlaySfx(GameManager.Instance.EnemyDeathSound);
            
            // FIX: Ensure the board knows this enemy is GONE
            var cellData = GameManager.Instance.BoardManager.GetCellData(m_Cell);
            
            // Restore food if it exists and hasn't been eaten, otherwise set to null
            if (m_UnderlyingObject != null && m_UnderlyingObject.gameObject.activeInHierarchy)
            {
                cellData.ContainedObject = m_UnderlyingObject;
            }
            else
            {
                cellData.ContainedObject = null;
            }
            
            PoolManager.Instance.ReturnToPool(gameObject);
        }
        return false; // Blocks player movement while health > 0
    }

    private void TurnHappened()
    {
        var playerCell = GameManager.Instance.PlayerController.Cell;
        
        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        // Attack if the player is in an adjacent cell
        if (absXDist + absYDist == 1)
        {
            Attack();
        }
        else
        {
            // Simple pathfinding: Priority given to the longest distance axis
            bool moved = false;
            if (absXDist > absYDist)
            {
                moved = TryMoveInX(xDist);
                if (!moved) moved = TryMoveInY(yDist);
            }
            else
            {
                moved = TryMoveInY(yDist);
                if (!moved) moved = TryMoveInX(xDist);
            }
        }
    }

    private void Attack()
    {
        m_Animator.SetTrigger("Attack");
        GameManager.Instance.PlaySfx(GameManager.Instance.EnemyAttackSound);
        GameManager.Instance.ChangeFood(-m_FoodDamage);
        
        Debug.Log("Enemy attacked the player!");
    }

    private bool TryMoveInX(int xDist)
    {
        return MoveTo(m_Cell + new Vector2Int(xDist > 0 ? 1 : -1, 0));
    }

    private bool TryMoveInY(int yDist)
    {
        return MoveTo(m_Cell + new Vector2Int(0, yDist > 0 ? 1 : -1));
    }

private bool MoveTo(Vector2Int coord)
{
    var board = GameManager.Instance.BoardManager;
    var targetCell = board.GetCellData(coord);

    if (targetCell == null || !targetCell.Passable) return false;

    if (targetCell.ContainedObject != null && !(targetCell.ContainedObject is FoodObject))
    {
        return false;
    }

    // Only restore the old object if it's still active (not collected)
    if (m_UnderlyingObject != null && m_UnderlyingObject.gameObject.activeInHierarchy)
    {
        board.GetCellData(m_Cell).ContainedObject = m_UnderlyingObject;
    }
    else
    {
        board.GetCellData(m_Cell).ContainedObject = null;
    }

    m_UnderlyingObject = targetCell.ContainedObject;
    targetCell.ContainedObject = this;
    m_Cell = coord;

    m_MoveTarget = board.CellToWorld(coord);
    m_IsMoving = true;
    m_Animator.SetBool("Moving", true);

    return true;
}
}