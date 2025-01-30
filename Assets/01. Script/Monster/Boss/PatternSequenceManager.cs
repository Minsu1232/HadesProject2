// 새로운 매니저 클래스
using System.Collections.Generic;
using DG.Tweening;
public static class PatternSequenceManager
{
    private static HashSet<Sequence> activeSequences = new HashSet<Sequence>();

    public static void RegisterSequence(Sequence sequence)
    {
        if (sequence != null)
        {
            activeSequences.Add(sequence);
            // 시퀀스가 완료되면 자동으로 제거
            sequence.OnComplete(() => activeSequences.Remove(sequence));
        }
    }

    public static void ClearAllSequences()
    {
        foreach (var sequence in activeSequences)
        {
            if (sequence != null && sequence.IsActive())
            {
                sequence.Kill();
            }
        }
        activeSequences.Clear();
    }
}