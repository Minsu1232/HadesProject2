public interface IRageSystem
{
    // Properties
    float CurrentRage { get; }
    float MaxRage { get; }
    bool IsInRage { get; }

    // Core methods
    void IncreaseRage(float amount);
    void DecreaseRage(float amount);
    void UpdateRage();

    // Events for state changes
    event System.Action<float> OnRageChanged;       // float parameter represents new rage value
    event System.Action OnRageStateChanged;         // Triggered when entering/exiting rage state
}