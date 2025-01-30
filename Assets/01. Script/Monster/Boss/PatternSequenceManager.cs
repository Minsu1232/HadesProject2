// ���ο� �Ŵ��� Ŭ����
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
            // �������� �Ϸ�Ǹ� �ڵ����� ����
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