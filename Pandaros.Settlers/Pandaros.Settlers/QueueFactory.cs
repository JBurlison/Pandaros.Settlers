using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandaros.Settlers
{
    public class QueueFactory<T> : IDisposable
    {
        Queue<T> _executeRequests = new Queue<T>();
        CancellationTokenSource _tokenSource = new CancellationTokenSource();
        TaskFactory _taskFoctory;
        List<Task> _runningTasks = new List<Task>();
        bool _running = false;
        Thread _queueManager;
        AutoResetEvent _processQueueSemaphore = new AutoResetEvent(false);
        AutoResetEvent _allWorkersBusySemaphore = new AutoResetEvent(false);
        AutoResetEvent _shutdownSemaphore = new AutoResetEvent(false);
        int _maxWorkers;
        bool _disposed = false;
        T _defaultValue = default(T);

        /// <summary>
        ///     Called whenever a worker is to execute work on a queued object.
        /// </summary>
        public event EventHandler<T> DoWork;

        public Guid UID { get; } = Guid.NewGuid();

        public string Name { get; } = string.Empty;

        /// <summary>
        ///     QueueFactory asynchronously events out data fifo. 
        ///     This class allows you to control the thread pool size of the worker items while maintaining data throughput.
        ///     The factory will only use the number of threads it needs up to the maximum.
        ///     Idle threads will be timed out after the workerThreadTimeoutSeconds has elapsed.
        /// </summary>
        /// <param name="name">The friendly name of the instance of this class</param>
        /// <param name="maxFactoryWorkerThreads">maximum number of threads allowed.</param>
        /// <param name="workerThreadTimeoutSeconds">Idle threads will be timed out after the workerThreadTimeoutSeconds has elapsed.</param>
        public QueueFactory(string name, int maxFactoryWorkerThreads = 20, int workerThreadTimeoutSeconds = 60)
        {
            _maxWorkers = maxFactoryWorkerThreads;
            _taskFoctory = new TaskFactory(_tokenSource.Token);
        }

        /// <summary>
        ///     Starts processing the Queue
        /// </summary>
        public void Start()
        {
            if (_running)
                return;

            _running = true;

            _queueManager = new Thread(new ThreadStart(QueueManager));
            _queueManager.IsBackground = true;
            _queueManager.Start();
        }

        /// <summary>
        ///     Clears any queued items and stops processing the queue.
        /// </summary>
        public void Stop()
        {
            try
            {
                _running = false;

                lock (_executeRequests)
                    _executeRequests.Clear();

                _tokenSource.Cancel();

                _shutdownSemaphore.WaitOne(5000); // wait up to 5 seconds for the QueueManager finish.

                lock (_runningTasks)
                    _runningTasks.Clear();
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }

        /// <summary>
        ///     Enqueues an object to be processed.
        /// </summary>
        /// <param name="request">the object requested to be worked by the queue factory</param>
        public void Enqueue(T request)
        {
            lock (_executeRequests)
                _executeRequests.Enqueue(request);

            _processQueueSemaphore.Set();
        }

        private void QueueManager()
        {
            while (_running && !_tokenSource.IsCancellationRequested)
            {
                try
                {
                    _processQueueSemaphore.WaitOne(1000);
                }
                catch { } // log no error in the case that this thread is aborted.

                try
                {
                    while (_executeRequests.Count > 0 && !_tokenSource.IsCancellationRequested)
                    {
                        T request = _defaultValue;
                        Task newTask = null;

                        // Before we dequeue a request we check to ensure we can process it.
                        // loop over all our workers to find one that is not busy.
                        lock (_runningTasks)
                        {
                            foreach (Task worker in _runningTasks)
                            {
                                if (!worker.IsCompleted)
                                    continue;

                                _runningTasks.Remove(worker);
                                break;
                            }

                            if (_runningTasks.Count < _maxWorkers)
                            {
                                try
                                {
                                    // Get the next request.
                                    lock (_executeRequests)
                                        request = _executeRequests.Dequeue();
                                }
                                catch { } // log no error in the case there is nothing to dequeue

                                if (request != null && !request.Equals(_defaultValue))
                                {
                                    newTask = _taskFoctory.StartNew(() =>
                                    {
                                        try
                                        {
                                            DoWork?.Invoke(this, request);
                                        }
                                        catch (Exception ex)
                                        {
                                            PandaLogger.LogError(ex, "Error on invoking Do work in queue factory. {0}", GetName());
                                        }
                                    });

                                    _runningTasks.Add(newTask);
                                }
                            }
                        }

                        if (newTask == null)
                            _allWorkersBusySemaphore.WaitOne(100); // so we do not get into a tight loop if all workers are busy.
                    }
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
            }

            _shutdownSemaphore.Set();
        }

        private string GetName()
        {
            return string.IsNullOrEmpty(Name) ? UID.ToString() : Name;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool dispose)
        {
            if (dispose && !_disposed)
            {
                _disposed = true;
                Stop();

                _processQueueSemaphore.Dispose();
                _allWorkersBusySemaphore.Dispose();
                _shutdownSemaphore.Dispose();
            }
        }
    }
}
