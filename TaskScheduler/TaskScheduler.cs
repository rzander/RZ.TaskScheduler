using System.Runtime.CompilerServices;

namespace RZ.TaskScheduler
{
    /// <summary>
    /// The scheduler is a simple class that allows you to schedule tasks to run at a specific time interval.
    /// </summary>
    public sealed class Scheduler
    {
        /// <summary>
        /// The list of scheduled tasks.
        /// </summary>
        public static List<ScheduledTask> ScheduledTasks = new List<ScheduledTask>();

        /// <summary>
        /// Adds a scheduled task.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="timerCallback"></param>
        /// <returns></returns>
        public static ScheduledTask Add(string Name, TimerCallback timerCallback)
        {
            var rt = new ScheduledTask
            {
                Name = Name,
                TimerCallback = timerCallback
            };

            if (ScheduledTasks.FirstOrDefault(x => x.Name == Name) != null)
            {
                var existing = ScheduledTasks.First(x => x.Name == Name);
                existing.Timer?.Dispose();
                ScheduledTasks.Remove(existing);
            }
            ScheduledTasks.Add(rt);

            return rt;
        }

        /// <summary>
        /// Gets a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static ScheduledTask? Get(string Name)
        {
            return ScheduledTasks.FirstOrDefault(x => x.Name == Name);
        }

        /// <summary>
        /// Removes a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static bool Remove(string Name)
        {
            var existing = ScheduledTasks.FirstOrDefault(x => x.Name == Name);
            if (existing != null)
            {
                existing.Timer?.Dispose();
                ScheduledTasks.Remove(existing);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops a scheduled task by name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static bool Stop(string Name)
        {
            var existing = ScheduledTasks.FirstOrDefault(x => x.Name == Name);
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
        public static bool Run(string Name)
        {
            var existing = ScheduledTasks.FirstOrDefault(x => x.Name == Name);
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
        public static bool Run(string Name, bool singleinstance, bool wait = false, TimeSpan? timeout = null)
        {
            if (singleinstance)
            {
                var existing = ScheduledTasks.FirstOrDefault(x => x.Name == Name);
                if (existing != null)
                {
                    existing.Run(singleinstance, wait, timeout);
                }

            }
            else return Run(Name);

            return false;
        }

        /// <summary>
        /// Cleans up the scheduled tasks that have already run.
        /// </summary>
        public static void Cleanup()
        {
            foreach (var oTask in ScheduledTasks.Where(x => x.NextRun < DateTime.Now).ToList())
            {
                try
                {
                    oTask.Timer?.Dispose();
                    ScheduledTasks.Remove(oTask);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// Represents a scheduled task.
    /// </summary>
    public class ScheduledTask
    {
        private TaskFactory _factory;
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
        public ScheduledTask Every(TimeSpan timeSpan, bool skipstartevent, bool pause = false, bool singleinstance = false)
        {
            int dueTime = skipstartevent ? (int)timeSpan.TotalMilliseconds : 0;

            if (pause) dueTime = -1;

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + new TimeSpan(0, 0, 0, 0, dueTime);
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Scheduler.Run(Name, singleinstance, false, timeSpan); }, this, dueTime, (int)timeSpan.TotalMilliseconds);

            return this;
        }

        /// <summary>
        /// Schedule the task to run at a specific time interval.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public ScheduledTask Every(TimeSpan? timeSpan, TimeSpan? delay = null, bool singleinstance = false)
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
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Scheduler.Run(Name, singleinstance, false, timeSpan); }, this, dueTime, period);

            return this;
        }

        /// <summary>
        /// Schedule the task to run once.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public ScheduledTask Once(TimeSpan? delay = null, bool singleinstance = false)
        {
            int dueTime = 0;
            if (delay != null) dueTime = (int)delay.Value.TotalMilliseconds;

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + new TimeSpan(0, 0, 0, 0, dueTime);
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Scheduler.Run(Name, singleinstance, false); }, this, dueTime, -1);

            return this;
        }

        /// <summary>
        /// Schedule the task to run once.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public ScheduledTask Once(DateTime startTime, bool singleinstance = false)
        {
            var dueTime = (startTime - DateTime.Now);

            if (Timer != null)
            {
                Timer.Dispose();
            }

            _nextRun = DateTime.Now + dueTime;
            Timer = new Timer((e) => { _lastRun = DateTime.Now; Scheduler.Run(Name, singleinstance, false); }, this, dueTime, Timeout.InfiniteTimeSpan);

            return this;
        }

        public ScheduledTask Stop()
        {
            if (Timer != null)
            {
                Timer.Dispose();
            }

            Result = null;
            _isRunning = false;
            _isCompleted = false;
            _nextRun = new DateTime();
            return this;
        }

        /// <summary>
        /// Run the task.
        /// </summary>
        public bool Run(CancellationTokenSource? cancellationTokenSource = null, TimeSpan? MaxRunTime = null)
        {
            _isRunning = true;
            _isCompleted = false;

            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            if(MaxRunTime != null)
            {
                cancellationTokenSource.CancelAfter((TimeSpan)MaxRunTime);
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
                    
                    if(CancellationToken.IsCancellationRequested || cancellationTokenSource.Token.IsCancellationRequested)
                        _isCompleted = false;
                    else
                        _isCompleted = true;

                    _isRunning = false;
                }
                catch (Exception ex)
                {
                    if (_onError != null)
                        _onError(new ScheduledTaskException(ex, this));
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
                    _onError(new ScheduledTaskException(ex, this));
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
        public bool Run(bool singleinstance = true, bool wait = false, TimeSpan? timeout = null, CancellationTokenSource? cancellationTokenSource = null, TimeSpan? MaxRunTime = null)
        {
            _isRunning = true;
            _isCompleted = false;

            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            if (MaxRunTime != null)
            {
                cancellationTokenSource.CancelAfter((TimeSpan)MaxRunTime);
            }

            CancellationToken = cancellationTokenSource.Token;

            if (singleinstance)
            {
                if (timeout == null)
                    timeout = TimeSpan.FromMilliseconds(1000);

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
                                _onError(new ScheduledTaskException(ex, this));
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
                            _onError(new ScheduledTaskException(ex, this));
                        _isCompleted = false;
                    }
                }
                _isRunning = false;
                return true;
            }
            else return Run(cancellationTokenSource);
        }

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
                                _onError(new ScheduledTaskException(ex, this));
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
                            _onError(new ScheduledTaskException(ex, this));
                        return false;
                    }
                }, cancellationTokenSource.Token);
            }
        }

        public ScheduledTask OnError(TimerCallback onError)
        {
            _onError = onError;
            return this;
        }

        public ScheduledTask OnComplete(TimerCallback onComplete)
        {
            _onComplete = onComplete;
            return this;
        }

        public void Queue(CancellationTokenSource? cancellationTokenSource = null)
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            CancellationToken = cancellationTokenSource.Token;

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 1, CancellationToken = cancellationTokenSource.Token };
            Parallel.Invoke(options, () => { Run(cancellationTokenSource); });
        }
    }

    public class ScheduledTaskException : Exception
    {
        public ScheduledTaskException(Exception ex, ScheduledTask st)
        {
            Exception = ex;
            ScheduledTask = st;
        }
        public Exception Exception { get; set; }

        public ScheduledTask ScheduledTask { get; set; }
    }

}