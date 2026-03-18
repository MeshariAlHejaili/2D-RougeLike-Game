using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    [Range(0.01f, 0.5f)]
    [Tooltip("Time (in seconds) it takes to slide from one cell to the next.")]
    private float m_MoveDuration = 0.1f;

    [Header("References")]
    [SerializeField]
    [Tooltip("Animator controlling Idle/Walk/Attack states.")]
    private Animator m_Animator;
    
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private bool m_IsGameOver;
    private bool m_IsMoving;
    private Coroutine m_MoveCoroutine;

    public Vector2Int Cell => m_CellPosition;

    private void Awake()
    {
        if (m_Animator == null) m_Animator = GetComponent<Animator>();
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        MoveTo(cell, true);
    }

    public void MoveTo(Vector2Int cell, bool immediate)
    {
        // Capture what is in the cell BEFORE the slide starts
        CellObject targetObject = m_Board.GetCellData(cell).ContainedObject;
        
        m_CellPosition = cell;
        Vector3 targetWorldPos = m_Board.CellToWorld(m_CellPosition);

        if (immediate)
        {
            if (m_MoveCoroutine != null)
            {
                StopCoroutine(m_MoveCoroutine);
                m_MoveCoroutine = null;
            }
            transform.position = targetWorldPos;
            m_IsMoving = false;
            if (targetObject != null) targetObject.PlayerEntered();
        }
        else
        {
            // Play movement SFX only for successful moves (not bumps/blocked attempts)
            GameManager.Instance.PlaySfx(GameManager.Instance.MoveSound);

            // Use the version with the capturedObject parameter
            if (m_MoveCoroutine != null) StopCoroutine(m_MoveCoroutine);
            m_MoveCoroutine = StartCoroutine(SmoothMovement(targetWorldPos, targetObject));
        }

        m_Animator.SetBool("Moving", m_IsMoving);
    }

    private IEnumerator SmoothMovement(Vector3 targetPos, CellObject capturedObject)
    {
        m_IsMoving = true;
        m_Animator.SetBool("Moving", true);

        float elapsedTime = 0;
        Vector3 startingPos = transform.position;

        while (elapsedTime < m_MoveDuration)
        {
            transform.position = Vector3.Lerp(startingPos, targetPos, elapsedTime / m_MoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        m_IsMoving = false;
        m_Animator.SetBool("Moving", false);

        // Logic triggers at the END of the slide
        if (capturedObject != null && capturedObject.gameObject.activeInHierarchy)
        {
            capturedObject.PlayerEntered();
        }

        m_MoveCoroutine = null;
    }

    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame) GameManager.Instance.StartNewGame();
            return;
        }

        if (m_IsMoving) return;

        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

        if(Keyboard.current.upArrowKey.wasPressedThisFrame) { newCellTarget.y += 1; hasMoved = true; }
        else if(Keyboard.current.downArrowKey.wasPressedThisFrame) { newCellTarget.y -= 1; hasMoved = true; }
        else if(Keyboard.current.rightArrowKey.wasPressedThisFrame) { newCellTarget.x += 1; hasMoved = true; }
        else if(Keyboard.current.leftArrowKey.wasPressedThisFrame) { newCellTarget.x -= 1; hasMoved = true; }

        if(hasMoved)
        {
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

            if(cellData != null && cellData.Passable)
            {
                // Tick moves enemies BEFORE the player starts sliding
                GameManager.Instance.TurnManager.Tick();

                if (cellData.ContainedObject == null)
                {
                    MoveTo(newCellTarget, false);
                }
                else if(cellData.ContainedObject.PlayerWantsToEnter())
                {
                    MoveTo(newCellTarget, false);
                }
            }
        }
    }

    public void Attack()
    {
        m_Animator.SetTrigger("Attack");
        GameManager.Instance.PlaySfx(GameManager.Instance.AttackSound);
    }

    public void GameOver() { m_IsGameOver = true; }
    public void Init() { m_IsGameOver = false; }
}