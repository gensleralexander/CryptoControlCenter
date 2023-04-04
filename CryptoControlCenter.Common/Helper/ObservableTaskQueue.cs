using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoControlCenter.Common.Helper
{
    public class ObservableTaskQueue : AbstractPropertyChanged
    {
        public event Action StartWorking;
        public event Action StopWorking;

        private TaskQueue queue = new TaskQueue();
        private int tasksQueuedSinceQueueWasLastIdle = 0;
        private int pendingTasks = 0;
        public int Progress
        {
            get
            {
                if (tasksQueuedSinceQueueWasLastIdle == 0)
                    return 0;

                int numberOfCompletedTasks = tasksQueuedSinceQueueWasLastIdle - pendingTasks;
                return numberOfCompletedTasks * 100 / tasksQueuedSinceQueueWasLastIdle;
            }
        }
        public TimeSpan TimeBetweenTaskExecutions
        {
            get { return queue.TimeBetweenTaskExecutions; }
            set { queue.TimeBetweenTaskExecutions = value; }
        }
        public int NumberOfRetries
        {
            get { return queue.NumberOfRetries; }
            set { queue.NumberOfRetries = value; }
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            Interlocked.Increment(ref tasksQueuedSinceQueueWasLastIdle);
            if (Interlocked.Increment(ref pendingTasks) == 1)
                StartWorking?.Invoke();
            OnPropertyChanged("Progress");
            try
            {
                return await queue.Enqueue(taskGenerator).ConfigureAwait(false);
            }
            finally
            {
                OnPropertyChanged("Progress");
                if (Interlocked.Decrement(ref pendingTasks) == 0)
                {
                    Interlocked.Exchange(ref tasksQueuedSinceQueueWasLastIdle, 0);
                    StopWorking?.Invoke();
                }
            }
        }

        public Task Enqueue(Func<Task> taskGenerator)
        {
            return Enqueue(() => TaskUtilities.WithResult(taskGenerator(), true));
        }
    }



    public class TaskQueue
    {
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        public TimeSpan TimeBetweenTaskExecutions { get; set; }
        public int NumberOfRetries { get; set; }

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            await semaphore.WaitAsync();

            int numberOfTriesRemaining = NumberOfRetries + 1;
            Task delay = null;
            try
            {
                Func<Task<T>> wrappedGenerator = () =>
                {
                    delay = Task.Delay(TimeBetweenTaskExecutions);
                    return taskGenerator();
                };
                return await TaskUtilities.RetryOnFailure(wrappedGenerator, NumberOfRetries, TimeBetweenTaskExecutions)
                    .ConfigureAwait(false);
            }
            finally
            {
                ReleaseAfterDelay(delay);
            }
        }

        public Task Enqueue(Func<Task> taskGenerator)
        {
            return Enqueue(() => TaskUtilities.WithResult(taskGenerator(), true));
        }

        private async void ReleaseAfterDelay(Task delay)
        {
            try
            {
                await delay
                        .ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    public static class TaskUtilities
    {
        public static async Task<T> WithResult<T>(Task task, T value)
        {
            await task.ConfigureAwait(false);
            return value;
        }

        public static async Task<T> RetryOnFailure<T>(Func<Task<T>> taskGenerator,
            int numberOfRetries = 1, TimeSpan? timeBetweenExecutions = null)
        {
            int numberOfTriesRemaining = numberOfRetries + 1;
            while (true)
            {
                var delayTask = Task.Delay(timeBetweenExecutions ?? TimeSpan.Zero);
                try
                {
                    return await taskGenerator();
                }
                catch
                {
                    numberOfTriesRemaining--;
                    if (numberOfTriesRemaining == 0)
                        throw;
                    await delayTask;
                }
            }
        }
    }
}