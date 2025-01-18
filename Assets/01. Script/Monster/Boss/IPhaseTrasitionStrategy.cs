public interface IPhaseTransitionStrategy
{
    void StartTransition();
    void UpdateTransition();
    bool IsTransitionComplete { get; }
}