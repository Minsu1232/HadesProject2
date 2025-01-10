using System.Threading.Tasks;
using UnityEngine;

public static class TaskExtensions
{
    public static void Forget(this Task task)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Debug.LogError($"Async operation failed: {t.Exception}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}