using UnityEngine;

public abstract class BossPatternBase : IBossPattern
{
    protected BossAI boss;
    protected PhaseData phaseData;
    protected float patternTimer;

    public virtual void Initialize(BossAI boss, PhaseData phaseData)
    {
        this.boss = boss;
        this.phaseData = phaseData;
        patternTimer = 0f;
    }

    public virtual void Execute()
    {
        patternTimer += Time.deltaTime;
        if (patternTimer >= phaseData.patternChangeTime)
        {
            patternTimer = 0f;
            OnPatternEnd();
        }
    }

    protected virtual void OnPatternEnd() { }
    public virtual void OnPhaseStart()
    {
        if (phaseData.phaseStartEffect != null)
        {
            GameObject.Instantiate(phaseData.phaseStartEffect, boss.transform.position, Quaternion.identity);
        }
    }

    public virtual void OnPhaseEnd()
    {
        // 이펙트 정리 등
    }

    public virtual bool CanTransition() => true;
}