using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static Queue<Action> tasks = new Queue<Action>();

    static public void QueueTask(Action action)
    {
        lock (tasks)
        {
            tasks.Enqueue(action);
        }
    }

    private void Update()
    {
        if (tasks.Count > 0)
        {
            Action action = null;
            lock (tasks)
            {
                action = tasks.Dequeue();
            }
            if (action != null)
            {
                action();
            }
        }
    }
}
