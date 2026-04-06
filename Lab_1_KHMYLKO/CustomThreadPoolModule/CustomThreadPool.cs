namespace CustomThreadPoolModule
{
    public class CustomThreadPool
    {
        private readonly Queue<Action> _taskQueue = new Queue<Action>();
        private readonly List<Thread> _workers = new List<Thread>();

        private readonly object _lock = new object();
        private readonly Semaphore _taskSignal = new Semaphore(0, int.MaxValue);
        private Dictionary<Thread, DateTime> _heartbeat = new();
        private int _minThreads;
        private int _maxThreads;
        private int _currentThreads;

        private bool _isRunning = true;

        private const int IdleTimeout = 2000; 




        public CustomThreadPool(int minThreads, int maxThreads)
        {
            _minThreads = minThreads;
            _maxThreads = maxThreads;

            for (int i = 0; i < _minThreads; i++)
            {
                CreateWorker();
            }

            new Thread(() =>
            {
                while (_isRunning)
                {
                    Thread.Sleep(500);
                    lock (_lock)
                    {
                        Console.WriteLine($"[POOL] Threads: {_currentThreads}, Queue: {_taskQueue.Count}");
                    }
                }
            })
            { IsBackground = true }.Start();
            new Thread(MonitorThreads) { IsBackground = true }.Start();
        }

        private void Worker()
        {
            var currentThread = Thread.CurrentThread;

            while (_isRunning)
            {
                lock (_lock)
                {
                    _heartbeat[currentThread] = DateTime.Now;
                }

                if (!_taskSignal.WaitOne(IdleTimeout))
                {
                    TryScaleDown(currentThread);
                    return;
                }

                Action task = null;

                lock (_lock)
                {
                    if (_taskQueue.Count > 0)
                        task = _taskQueue.Dequeue();
                }

                try
                {
                    task?.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Task error: {ex.Message}");
                }

                lock (_lock)
                {
                    _heartbeat[currentThread] = DateTime.Now;
                }
            }
        }
        public void Enqueue(Action task)
        {
            lock (_lock)
            {
                _taskQueue.Enqueue(task);
            }

            _taskSignal.Release();

            TryScaleUp();
        }

        private void TryScaleUp()
        {
            lock (_lock)
            {
                if (_taskQueue.Count > _currentThreads && _currentThreads < _maxThreads)
                {
                    CreateWorker();
                }
            }
        }

        private void TryScaleDown(Thread thread)
        {
            lock (_lock)
            {
                if (_currentThreads > _minThreads)
                {
                    _workers.Remove(thread);
                    _heartbeat.Remove(thread); 
                    _currentThreads--;

                    Console.WriteLine($"Thread removed. Total: {_currentThreads}");
                }
            }
        }
        private void CreateWorker()
        {
            var thread = new Thread(Worker);

            lock (_lock)
            {
                _workers.Add(thread);
                _heartbeat[thread] = DateTime.Now;
                _currentThreads++;
            }

            Console.WriteLine($"Thread created. Total: {_currentThreads}");

            thread.Start();
        }

        public void Stop()
        {
            _isRunning = false;

            for (int i = 0; i < _workers.Count; i++)
                _taskSignal.Release();
        }

        private void MonitorThreads()
        {
            while (_isRunning)
            {
                Thread.Sleep(1000);

                List<Thread> toReplace = new();

                lock (_lock)
                {
                    foreach (var kvp in _heartbeat.ToList())
                    {
                        if ((DateTime.Now - kvp.Value).TotalSeconds > 5)
                        {
                            toReplace.Add(kvp.Key);
                            _workers.Remove(kvp.Key);
                            _heartbeat.Remove(kvp.Key);
                            _currentThreads--;
                        }
                    }
                }

                foreach (var _ in toReplace)
                {
                    Console.WriteLine("Thread hung → replacing");
                    CreateWorker();
                }
            }
        }
    }
}

      
