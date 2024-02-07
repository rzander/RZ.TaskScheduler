using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RZ.TaskScheduler
{
    /// <summary>
    /// The RZScheduler is a simple class that allows you to schedule tasks to run at a specific time interval.
    /// </summary>
    public class RZScheduler
    {
        public RZScheduler() { }

        /// <summary>
        /// The list of scheduled tasks.
        /// </summary>
        public List<RZTask> RZTasks = new List<RZTask>();

        private LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(1);
        private TaskFactory factory;
        private object lockObj = new Object();

        /// <summary>
        /// Enqueues a scheduled task.
        /// </summary>
        /// <param name="task"></param>
        public void Queue(RZTask task)
        {
            factory = new TaskFactory(lcts);
            CancellationTokenSource cts = new CancellationTokenSource();

            Task t = factory.StartNew(() => {
                lock (lockObj)
                {
                    task.Run(singleinstance: true, wait: true, timeout: new TimeSpan(1,0,0));
                }
            }, cts.Token);
        }

        /// <summary>
        /// Adds a scheduled task.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="timerCallback"></param>
        /// <returns></returns>
        public RZTask? Add(string Name, TimerCallback timerCallback)
        {
            var rt = new RZTask
            {
                Name = Name,
                TimerCallback = timerCallback
            };

            if (RZTasks.FirstOrDefault(x => x.Name == Name) == null)
            {
                RZTasks.Add(rt);
                return rt;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Updates a scheduled task.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="timerCallback"></param>
        /// <returns></returns>
        public RZTask Update(string Name, TimerCallback timerCallback)
        {
            var rt = new RZTask
            {
                Name = Name,
                TimerCallback = timerCallback
            };

            if (RZTasks.FirstOrDefault(x => x.Name == Name) != null)
            {
                var existing = RZTasks.First(x => x.Name == Name);
                existing.Timer?.Dispose();
                RZTasks.Remove(existing);
            }
            RZTasks.Add(rt);

            return rt;
        }

        /// <summary>
        /// Gets a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public RZTask? Get(string Name)
        {
            return RZTasks.FirstOrDefault(x => x.Name == Name);
        }

        /// <summary>
        /// Removes a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public bool Remove(string Name)
        {
            var existing = RZTasks.FirstOrDefault(x => x.Name == Name);
            if (existing != null)
            {
                existing.Timer?.Dispose();
                RZTasks.Remove(existing);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public bool Stop(string Name)
        {
            var existing = RZTasks.FirstOrDefault(x => x.Name == Name);
            if (existing != null)
            {
                existing.Timer?.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calls the scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        public bool Run(string Name)
        {
            var existing = RZTasks.FirstOrDefault(x => x.Name == Name);
            if (existing != null)
            {
                new Task(() =>
                {
                    existing.Run(null);
                }).Start();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Calls the scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="singleinstance"></param>
        /// <param name="wait"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Run(string Name, bool singleinstance, bool wait = false, TimeSpan? timeout = null)
        {
            if (singleinstance)
            {
                var existing = RZTasks.FirstOrDefault(x => x.Name == Name);
                if (existing != null)
                {
                    existing.Run(singleinstance, wait, timeout);
                }

            }
            else return Run(Name);

            return false;
        }

        /// <summary>
        /// cleans up the scheduled tasks that have already run.
        /// </summary>
        public void Cleanup(bool Completed = true, bool NoSchedule = false)
        {
            if (Completed)
            {
                foreach (var oTask in RZTasks.Where(x => x.IsCompleted).ToList())
                {
                    try
                    {
                        oTask.Timer?.Dispose();
                        RZTasks.Remove(oTask);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            if (NoSchedule)
            {
                foreach (var oTask in RZTasks.Where(x => x.NextRun < DateTime.Now).ToList())
                {
                    try
                    {
                        oTask.Timer?.Dispose();
                        RZTasks.Remove(oTask);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }

    public sealed class RZSched : RZScheduler
    {
        /// <summary>
        /// The instance of the RZSched.
        /// </summary>
        private static RZScheduler _instance = new RZScheduler();

        /// <summary>
        /// Creates a new instance of the RZSched.
        /// </summary>
        public RZSched() : base()
        {
            _instance = new RZScheduler();
        }

        /// <summary>
        /// list of scheduled tasks.
        /// </summary>
        public new static List<RZTask> RZTasks
        {
            get { return _instance.RZTasks; }
        }

        /// <summary>
        /// enqueues a scheduled task.
        /// </summary>
        /// <param name="task"></param>
        public new static void Queue(RZTask task)
        {
            _instance.Queue(task);
        }

        /// <summary>
        /// adds a scheduled task, skip if task exists.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="timerCallback"></param>
        /// <returns></returns>
        public new static RZTask? Add(string Name, TimerCallback timerCallback)
        {
            return _instance.Add(Name, timerCallback);
        }

        /// <summary>
        /// add or updates a scheduled task.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="timerCallback"></param>
        /// <returns></returns>
        public new static RZTask Update(string Name, TimerCallback timerCallback)
        {
            return _instance.Update(Name, timerCallback);
        }

        /// <summary>
        /// gets a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public new static RZTask? Get(string Name)
        {
            return _instance.Get(Name);
        }

        /// <summary>
        /// removes a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public new static bool Remove(string Name)
        {
            return _instance.Remove(Name);
        }

        /// <summary>
        /// stops a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public new static bool Stop(string Name)
        {
            return _instance.Stop(Name);
        }

        /// <summary>
        /// calls the scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public new static bool Run(string Name)
        {
            return _instance.Run(Name);
        }

        /// <summary>
        /// calls the scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="singleinstance"></param>
        /// <param name="wait"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public new static bool Run(string Name, bool singleinstance, bool wait = false, TimeSpan? timeout = null)
        {
            return _instance.Run(Name, singleinstance, wait, timeout);
        }

        /// <summary>
        /// cleans up the scheduled tasks that have already run.
        /// </summary>
        public new static void Cleanup(bool Completed = true, bool NoSchedule = false)
        {
            _instance.Cleanup(Completed, NoSchedule);
        }
    }

    /// <summary>
    /// Represents a scheduled task.
    /// </summary>
    public class RZTask
    {
        private DateTime _nextRun;
        internal DateTime _lastRun;
        private bool _isRunning = false;
        private bool _isCompleted = false;
        public required TimerCallback TimerCallback { get; set; }
        public required string Name { get; set; }
        public Timer? Timer { get; set; }
        private TimerCallback? _onError { get; set; }
        private TimerCallback? _onComplete { get; set; }
        public object? Result { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        public DateTime? NextRun
        {
            get { return _nextRun; }
        }

        public DateTime? LastRun
        {
            get { return _lastRun; }
        }

        /// <summary>
        /// Schedule the task to run at a specific time interval.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="skipstartevent"></param>
        /// <param name="pause"></param>
        /// <param name="singleinstance"></param>
        /// <returns></returns>
        public RZTask Every(TimeSpan timeSpan, bool skipstartevent, bool pause = false, bool singleinstance = false)
        {
            int dueTime = skipstartevent ? (int)timeSpan.TotalMilliseconds : 0;

            if (pause) dueTime = -1;

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + new TimeSpan(0, 0, 0, 0, dueTime);
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Run(singleinstance, false, timeSpan); }, this, dueTime, (int)timeSpan.TotalMilliseconds);

            return this;
        }

        /// <summary>
        /// Schedule the task to run at a specific time interval.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public RZTask Every(TimeSpan? timeSpan, TimeSpan? delay = null, bool singleinstance = false)
        {
            int dueTime = 0;
            int period = 0;
            if (delay != null) dueTime = (int)delay.Value.TotalMilliseconds;
            if (timeSpan != null) period = (int)timeSpan.Value.TotalMilliseconds; else period = -1;

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + new TimeSpan(0, 0, 0, 0, dueTime);
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Run(singleinstance, false, timeSpan); }, this, dueTime, period);

            return this;
        }

        /// <summary>
        /// Schedule the task to run once.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public RZTask Once(TimeSpan? delay = null, bool singleinstance = false)
        {
            int dueTime = 0;
            if (delay != null) dueTime = (int)delay.Value.TotalMilliseconds;

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + new TimeSpan(0, 0, 0, 0, dueTime);
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Run(singleinstance, false); }, this, dueTime, -1);

            return this;
        }

        /// <summary>
        /// Schedule the task to run once.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public RZTask Once(DateTime startTime, bool singleinstance = false)
        {
            var dueTime = (startTime - DateTime.Now);

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + dueTime;
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Run(singleinstance, false); }, this, dueTime, Timeout.InfiniteTimeSpan);

            return this;
        }

        /// <summary>
        /// Stop the task.
        /// </summary>
        /// <returns></returns>
        public RZTask Stop()
        {
            if (Timer != null)
            {
                Timer.Dispose();
            }

            CancellationToken = new CancellationToken(true);

            Result = null;
            _isRunning = false;
            _isCompleted = false;
            _nextRun = new DateTime();
            return this;
        }

        /// <summary>
        /// Run the task.
        /// </summary>
        public bool Run(CancellationTokenSource? cancellationTokenSource = null)
        {
            _isRunning = true;
            _isCompleted = false;

            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            CancellationToken = cancellationTokenSource.Token;

            var T1 = Task.Run(() =>
            {
                try
                {
                    _lastRun = DateTime.Now;
                    TimerCallback(this);
                    if (_onComplete != null && !CancellationToken.IsCancellationRequested && !cancellationTokenSource.Token.IsCancellationRequested)
                        _onComplete(this);

                    if (CancellationToken.IsCancellationRequested || cancellationTokenSource.Token.IsCancellationRequested)
                        _isCompleted = false;
                    else
                        _isCompleted = true;

                    _isRunning = false;
                }
                catch (Exception ex)
                {
                    if (_onError != null)
                        _onError(new RZTaskException(ex, this));
                    _isCompleted = false;
                }
                finally
                {

                    _isRunning = false;
                }
            }, cancellationTokenSource.Token);

            try
            {
                T1.Wait(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _isRunning = false;
                if (_onError != null)
                    _onError(new RZTaskException(ex, this));
                _isCompleted = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Run the task.
        /// </summary>
        /// <param name="singleinstance"></param>
        /// <param name="wait"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Run(bool singleinstance, bool wait = false, TimeSpan? timeout = null, CancellationTokenSource? cancellationTokenSource = null)
        {
            _isRunning = true;
            _isCompleted = false;

            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            CancellationToken = cancellationTokenSource.Token;

            if (singleinstance)
            {
                if (timeout == null)
                    timeout = new TimeSpan(1,0,0);

                Task tCall = Task.Run(() =>
                {
                    //check if Task is already running
                    if (Monitor.TryEnter(this, (TimeSpan)timeout))
                    {
                        try
                        {
                            _lastRun = DateTime.Now;
                            TimerCallback(this);
                            if (_onComplete != null && !CancellationToken.IsCancellationRequested && !cancellationTokenSource.Token.IsCancellationRequested)
                                _onComplete(this);

                            if (CancellationToken.IsCancellationRequested || cancellationTokenSource.Token.IsCancellationRequested)
                                _isCompleted = false;
                            else
                                _isCompleted = true;

                        }
                        catch (Exception ex)
                        {
                            if (_onError != null)
                                _onError(new RZTaskException(ex, this));
                            _isCompleted = false;
                        }
                        finally
                        {
                            Monitor.Exit(this);

                            _isRunning = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Task is already running...");
                    }
                }, cancellationTokenSource.Token);

                //wait for task to complete
                if (wait)
                {
                    try
                    {
                        tCall.Wait((TimeSpan)timeout, cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        if (_onError != null)
                            _onError(new RZTaskException(ex, this));
                        _isCompleted = false;
                        _isRunning = false;
                    }
                }

                return true;
            }
            else return Run(cancellationTokenSource);
        }

        /// <summary>
        /// run the task asynchronously.
        /// </summary>
        /// <param name="singleinstance"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task RunAsync(bool singleinstance = true, TimeSpan? timeout = null, CancellationTokenSource? cancellationTokenSource = null)
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            CancellationToken = cancellationTokenSource.Token;

            if (singleinstance)
            {
                await Task.Run(() =>
                {
                    if (timeout == null)
                        timeout = TimeSpan.FromMilliseconds(1000);

                    //check if Task is already running
                    if (Monitor.TryEnter(this, (TimeSpan)timeout))
                    {
                        try
                        {
                            _lastRun = DateTime.Now;
                            TimerCallback(this);
                            if (_onComplete != null && !CancellationToken.IsCancellationRequested && !cancellationTokenSource.Token.IsCancellationRequested)
                                _onComplete(this);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            if (_onError != null)
                                _onError(new RZTaskException(ex, this));
                            return false;
                        }
                        finally
                        {
                            Monitor.Exit(this);
                        }
                    }
                    return false;
                }, cancellationTokenSource.Token);
            }
            else
            {
                await Task.Run(() =>
                {
                    try
                    {
                        _lastRun = DateTime.Now;
                        TimerCallback(this);
                        if (_onComplete != null)
                            _onComplete(this);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (_onError != null)
                            _onError(new RZTaskException(ex, this));
                        return false;
                    }
                }, cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// handle to run in case of error.
        /// </summary>
        /// <param name="onError"></param>
        /// <returns></returns>
        public RZTask OnError(TimerCallback onError)
        {
            _onError = onError;
            return this;
        }

        /// <summary>
        /// handle to run when task is completed.
        /// </summary>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public RZTask OnComplete(TimerCallback onComplete)
        {
            _onComplete = onComplete;
            return this;
        }
    }

    public class RZTaskException : Exception
    {
        public RZTaskException(Exception ex, RZTask st)
        {
            Exception = ex;
            RZTask = st;
        }
        public Exception Exception { get; set; }

        public RZTask RZTask { get; set; }
    }

    public class LimitedConcurrencyLevelTaskScheduler : System.Threading.Tasks.TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        // The maximum concurrency level allowed by this scheduler.
        private readonly int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items.
        private int _delegatesQueuedOrRunning = 0;

        // Creates a new instance with the specified degree of parallelism.
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        // Queues a task to the scheduler.
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler.
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        // Attempts to execute the specified task on the current thread.
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task.
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler.
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // Gets the maximum concurrency level supported by this scheduler.
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        // Gets an enumerable of the tasks currently scheduled on this scheduler.
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }

    }
}