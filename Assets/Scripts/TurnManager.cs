public class TurnManager
{
    // Event triggered every time a turn passes
    public event System.Action OnTick;
    
    private int m_TurnCount;

    public int TurnCount => m_TurnCount;

    // Constructor to initialize the manager
    public TurnManager()
    {
        m_TurnCount = 1;
    }

    // Called whenever the player performs a valid action
    public void Tick()
    {
        // Increment the internal counter
        m_TurnCount += 1;

        // Invoke the OnTick event for subscribers (like Enemies)
        OnTick?.Invoke();
    }
}