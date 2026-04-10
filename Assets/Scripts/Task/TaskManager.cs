using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XmqqyBackpack
{
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }

        private readonly Queue<ITask> taskQueue = new Queue<ITask>();
        private ITask currentTask;
        private Coroutine executionCoroutine;

        public bool IsRunning { get; private set; }
        public bool IsPaused { get; set; }

        public event Action<ITask> OnTaskStarted;
        public event Action<ITask> OnTaskCompleted;
        public event Action OnQueueEmpty;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            BeginExecution();
        }

        public void BeginExecution()
        {
            if (!IsRunning && taskQueue.Count > 0)
            {
                IsRunning = true;
                executionCoroutine = StartCoroutine(ProcessQueue());
            }
        }

        public void AddTask(ITask task)
        {
            if (task == null)
            {
                Debug.LogWarning("尝试添加空任务到队列");
                return;
            }

            taskQueue.Enqueue(task);
            Debug.Log($"[TaskManager] 添加任务: {task.GetType().Name}");

            if (!IsRunning)
                BeginExecution();
        }

        public void AddTasks(IEnumerable<ITask> tasks)
        {
            foreach (var task in tasks)
                AddTask(task);
        }

        public void ClearQueue()
        {
            taskQueue.Clear();
            Debug.Log("[TaskManager] 队列已清空");
        }

        public void StopAndClear()
        {
            if (executionCoroutine != null)
                StopCoroutine(executionCoroutine);

            currentTask?.Cancel();
            currentTask = null;
            IsRunning = false;
            ClearQueue();
        }

        public void SkipCurrentTask()
        {
            if (currentTask != null)
            {
                Debug.Log($"[TaskManager] 跳过任务: {currentTask.GetType().Name}");
                currentTask.Cancel();
            }
        }

        private IEnumerator ProcessQueue()
        {
            while (taskQueue.Count > 0)
            {
                while (IsPaused)
                    yield return null;

                currentTask = taskQueue.Dequeue();
                OnTaskStarted?.Invoke(currentTask);

                Debug.Log($"[TaskManager] 开始执行任务: {currentTask.GetType().Name}");

                yield return StartCoroutine(currentTask.Execute());

                OnTaskCompleted?.Invoke(currentTask);
                Debug.Log($"[TaskManager] 完成任务: {currentTask.GetType().Name}");

                currentTask = null;
            }

            IsRunning = false;
            OnQueueEmpty?.Invoke();
            Debug.Log("[TaskManager] 队列已空");
        }

        private void OnDestroy()
        {
            StopAndClear();
        }
    }
}