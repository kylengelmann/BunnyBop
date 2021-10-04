using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CheatSystem;

public class GameplayQueue : Singleton<GameplayQueue>
{
    Queue<GameplayTask> GameplayTasks;

    public bool bIsProcessingTasks { get; private set; }

    Coroutine ProcessRoutine = null;
    Coroutine CurrentTaskRoutine = null;

    public void AddTask(GameplayTask Task)
    {
        GameplayTasks.Enqueue(Task);
    }

    public void StartProcessingTasks()
    {
        if (!bIsProcessingTasks)
        {
            bIsProcessingTasks = true;
            ProcessRoutine = StartCoroutine(ProcessTasks());
        }
    }

    public void StopProcessingTasks()
    {
        if (bIsProcessingTasks)
        {
            if (ProcessRoutine != null)
            {
                StopCoroutine(ProcessRoutine);
                ProcessRoutine = null;
            }

            if (CurrentTaskRoutine != null)
            {
                StopCoroutine(CurrentTaskRoutine);
                CurrentTaskRoutine = null;
            }
            ProcessRoutine = null;
            bIsProcessingTasks = false;
        }
    }

    public void ClearTasks()
    {
        GameplayTasks.Clear();
    }

    IEnumerator ProcessTasks()
    {
        while (true)
        {
            if (GameplayTasks.Count > 0)
            {
                GameplayTask Task = GameplayTasks.Dequeue();
                CurrentTaskRoutine = Task.TaskRoutine;
                yield return Task.TaskRoutine;
                CurrentTaskRoutine = null;
            }
            else
            {
                yield return null;
            }
        }
    }
}

public struct GameplayTask
{
    public Coroutine TaskRoutine;
}