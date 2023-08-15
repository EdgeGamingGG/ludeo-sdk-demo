using System.Collections.Generic;
using System;
using UnityEngine;

namespace LudeoSDK
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static object lockObject = new object();
        private static UnityMainThreadDispatcher instance;
        private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();

        #region ------------------ UNITY METHODS ------------------
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        private void Update()
        {
            lock (lockObject)
            {
                while (ExecutionQueue.Count > 0)
                {
                    ExecutionQueue.Dequeue().Invoke();
                }
            }
        }
        #endregion ------------------ UNITY METHODS ------------------

        public static void Enqueue(Action action)
        {
            lock (lockObject)
            {
                ExecutionQueue.Enqueue(action);
            }
        }

        private void Clear()
        {
            lock (lockObject)
            {
                ExecutionQueue.Clear();
            }
        }            
    }
}