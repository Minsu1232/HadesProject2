public interface ISkillStrategyComponentInjection
{
    void SetSkillEffect(ISkillEffect effect);
    void SetProjectileMovement(IProjectileMovement movement);
    void SetProjectileImpact(IProjectileImpact impact);
}
