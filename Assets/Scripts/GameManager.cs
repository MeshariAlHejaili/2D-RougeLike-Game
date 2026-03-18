using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem; // Ensure you have the Input System package

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Managers")]
    [SerializeField] private BoardManager m_BoardManager;
    [SerializeField] private PlayerController m_PlayerController;

    public BoardManager BoardManager => m_BoardManager;
    public PlayerController PlayerController => m_PlayerController;
    public TurnManager TurnManager { get; private set; }

    [Header("UI System")]
    [SerializeField] private UIDocument m_UIDoc;

    private Label m_FoodLabel;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    
    // References for the Bonus Menu Panels
    private VisualElement m_MainMenuPanel;
    private VisualElement m_PauseMenuPanel;

    [Header("Gameplay Balance")]
    [SerializeField] [Range(10, 100)] private int m_InitialFoodAmount = 20;

    private int m_FoodAmount;
    private int m_CurrentLevel = 1;

    // State tracking
    private bool m_IsPaused;
    private bool m_IsGameStarted;

    public int CurrentLevel => m_CurrentLevel;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource m_SfxSource;
    [SerializeField] private AudioClip m_MoveSound;
    [SerializeField] private AudioClip m_AttackSound;
    [SerializeField] private AudioClip m_FoodSound;
    [SerializeField] private AudioClip m_EnemyAttackSound;
    [SerializeField] private AudioClip m_EnemyDeathSound;
    [SerializeField] private AudioClip m_GameOverSound;

    public AudioClip MoveSound => m_MoveSound;
    public AudioClip AttackSound => m_AttackSound;
    public AudioClip FoodSound => m_FoodSound;
    public AudioClip EnemyAttackSound => m_EnemyAttackSound;
    public AudioClip EnemyDeathSound => m_EnemyDeathSound;
    public AudioClip GameOverSound => m_GameOverSound;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem m_WallDestroyPrefab;
    [SerializeField] private ParticleSystem m_FoodCollectPrefab;
    [SerializeField] private ParticleSystem m_EnemyDeathPrefab;

    public ParticleSystem WallDestroyPrefab => m_WallDestroyPrefab;
    public ParticleSystem FoodCollectPrefab => m_FoodCollectPrefab;
    public ParticleSystem EnemyDeathPrefab => m_EnemyDeathPrefab;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
        
        var root = m_UIDoc.rootVisualElement;

        // 1. Find existing HUD elements
        m_FoodLabel = root.Q<Label>("FoodLabel");
        m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");

        // 2. Find Menu Panels
        m_MainMenuPanel = root.Q<VisualElement>("MainMenuPanel");
        m_PauseMenuPanel = root.Q<VisualElement>("PauseMenuPanel");

        // 3. Wire up Button Click Events
        // Main Menu
        m_MainMenuPanel.Q<Button>("PlayButton").clicked += StartNewGame;
        m_MainMenuPanel.Q<Button>("QuitButton").clicked += () => 
        {
            #if UNITY_EDITOR
                // This stops the editor play mode
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                // This closes the actual game window in a build
                Application.Quit();
            #endif
        };
        
        // Pause Menu
        m_PauseMenuPanel.Q<Button>("ResumeButton").clicked += TogglePause;
        m_PauseMenuPanel.Q<Button>("RestartButton").clicked += StartNewGame;
        m_PauseMenuPanel.Q<Button>("MenuQuitButton").clicked += ShowMainMenu;

        // 4. Start at the Main Menu instead of starting the game immediately
        ShowMainMenu();
    }

    private void Update()
    {
        // 5. Listen for Pause Input (Escape)
        if (Keyboard.current.escapeKey.wasPressedThisFrame && m_IsGameStarted)
        {
            TogglePause();
        }
    }

    // Method to bring the user back to the title screen
    public void ShowMainMenu()
    {
        m_IsGameStarted = false;
        Time.timeScale = 0; // Freeze the game world
        
        m_MainMenuPanel.style.display = DisplayStyle.Flex;
        m_PauseMenuPanel.style.display = DisplayStyle.None;
        m_GameOverPanel.style.display = DisplayStyle.None;
    }

    //  Method to handle Pausing/Unpausing
    public void TogglePause()
    {
        m_IsPaused = !m_IsPaused;
        
        // If paused, timeScale is 0 (frozen). If not, it is 1 (normal speed).
        Time.timeScale = m_IsPaused ? 0 : 1;
        
        m_PauseMenuPanel.style.display = m_IsPaused ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void StartNewGame()
    {
        // 6. Ensure game state is clean
        m_IsGameStarted = true;
        m_IsPaused = false;
        Time.timeScale = 1;

        // Hide all menus
        m_MainMenuPanel.style.display = DisplayStyle.None;
        m_PauseMenuPanel.style.display = DisplayStyle.None;
        m_GameOverPanel.style.display = DisplayStyle.None;
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_CurrentLevel = 1;
        m_FoodAmount = m_InitialFoodAmount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;
        
        m_BoardManager.Clean();
        m_BoardManager.Init();
        
        m_PlayerController.Init();
        m_PlayerController.Spawn(m_BoardManager, new Vector2Int(1, 1));
    }

    private void OnTurnHappen() => ChangeFood(-1);

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            PlaySfx(m_GameOverSound);
            m_PlayerController.GameOver();
            
            // Show Game Over UI
            m_GameOverPanel.style.display = DisplayStyle.Flex;
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = "Game Over!\n\nSurvived " + m_CurrentLevel + " days\n(Press Enter)";
        }
    }

    public void NewLevel()
    {
        m_BoardManager.Clean();
        m_BoardManager.Init();
        m_PlayerController.Spawn(m_BoardManager, new Vector2Int(1, 1));
        m_CurrentLevel++;
    }

    // VFX and SFX Helpers remain the same
    public void PlayVFX(ParticleSystem prefab, Vector3 position)
    {
        if (prefab != null)
        {
            ParticleSystem instance = Instantiate(prefab, position, Quaternion.identity);
            instance.Play();
            Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
        }
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && m_SfxSource != null) m_SfxSource.PlayOneShot(clip);
    }
}